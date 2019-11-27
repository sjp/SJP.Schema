﻿using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteViewGenerator : DatabaseViewGenerator
    {
        public OrmLiteViewGenerator(INameTranslator nameTranslator, string baseNamespace)
            : base(nameTranslator)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = new[] { "ServiceStack.DataAnnotations" }
                .Union(
                    view.Columns
                        .Select(c => c.Type.ClrType.Namespace)
                        .Where(ns => ns != viewNamespace)
                )
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(viewNamespace));
            var classDeclaration = BuildClass(view, comment);

            var document = CompilationUnit()
                .WithUsings(new SyntaxList<UsingDirectiveSyntax>(usingStatements))
                .WithMembers(
                    new SyntaxList<MemberDeclarationSyntax>(
                        namespaceDeclaration.WithMembers(
                            new SyntaxList<MemberDeclarationSyntax>(classDeclaration)
                        )
                    )
                );

            using var workspace = new AdhocWorkspace();
            return Formatter.Format(document, workspace).ToFullString();
        }

        private ClassDeclarationSyntax BuildClass(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var className = NameTranslator.ViewToClassName(view.Name);
            var properties = view.Columns
                .Select(vc => BuildColumn(vc, comment, className))
                .ToList();

            return ClassDeclaration(className)
                .AddAttributeLists(BuildClassAttributes(view, className).ToArray())
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(BuildViewComment(view.Name, comment))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(properties));
        }

        private static IEnumerable<AttributeListSyntax> BuildClassAttributes(IDatabaseView view, string className)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var attributes = new List<AttributeListSyntax>();

            var schemaName = view.Name.Schema;
            if (!schemaName.IsNullOrWhiteSpace())
            {
                var schemaAttribute = AttributeList(
                    new SeparatedSyntaxList<AttributeSyntax>().Add(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(ServiceStack.DataAnnotations.SchemaAttribute)),
                            AttributeArgumentList(
                                SeparatedList(new[]
                                {
                                    AttributeArgument(
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(schemaName)))
                                })))));
                attributes.Add(schemaAttribute);
            }

            if (className != view.Name.LocalName)
            {
                var aliasAttribute = AttributeList(
                    new SeparatedSyntaxList<AttributeSyntax>().Add(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(ServiceStack.DataAnnotations.AliasAttribute)),
                            AttributeArgumentList(
                                SeparatedList(new[]
                                {
                                    AttributeArgument(
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(view.Name.LocalName)))
                                })))));
                attributes.Add(aliasAttribute);
            }

            return attributes;
        }

        private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IDatabaseViewComments> comment, string className)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var clrType = column.Type.ClrType;
            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

            var columnTypeSyntax = column.IsNullable
                ? NullableType(ParseTypeName(clrType.FullName))
                : ParseTypeName(clrType.FullName);
            if (clrType.Namespace == "System" && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
                columnTypeSyntax = column.IsNullable
                    ? NullableType(SyntaxUtilities.TypeSyntaxMap[clrType.Name])
                    : SyntaxUtilities.TypeSyntaxMap[clrType.Name];

            var baseProperty = PropertyDeclaration(
                columnTypeSyntax,
                Identifier(propertyName)
            );

            var columnSyntax = baseProperty
                .AddAttributeLists(BuildColumnAttributes(column, propertyName).ToArray())
                .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildColumnComment(column.Name, comment));

            var isNotNullRefType = !column.IsNullable && !column.Type.ClrType.IsValueType;
            if (!isNotNullRefType)
                return columnSyntax;

            return columnSyntax
                .WithInitializer(SyntaxUtilities.NotNullDefault)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static SyntaxTriviaList BuildViewComment(Identifier viewName, Option<IDatabaseViewComments> comment)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return comment
                .Bind(c => c.Comment)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("A mapping class to query the "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(viewName.LocalName))),
                        XmlText(" view.")
                    })
                );
        }

        private static SyntaxTriviaList BuildColumnComment(Identifier columnName, Option<IDatabaseViewComments> comment)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            return comment
                .Bind(c => c.ColumnComments.TryGetValue(columnName, out var cc) ? cc : Option<string>.None)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("The "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(columnName.LocalName))),
                        XmlText(" column.")
                    })
                );
        }

        private static IEnumerable<AttributeListSyntax> BuildColumnAttributes(IDatabaseColumn column, string propertyName)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(propertyName));

            if (propertyName == column.Name.LocalName)
                return Array.Empty<AttributeListSyntax>();

            return new[]
            {
                AttributeList(
                    SeparatedList(new[]
                    {
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(ServiceStack.DataAnnotations.AliasAttribute)),
                            AttributeArgumentList(
                                SeparatedList(new[]
                                {
                                    AttributeArgument(
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(column.Name.LocalName)))
                                }))
                        )
                    })
                )
            };
        }
    }
}

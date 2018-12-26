﻿using System;
using System.Linq;
using System.Collections.Generic;
using RazorLight.Razor;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html
{
    internal sealed class ReportingRazorProject : RazorLightProject
    {
        public ReportingRazorProject(ITemplateProvider templateProvider)
        {
            TemplateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
        }

        private ITemplateProvider TemplateProvider { get; }

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey) => _emptyItems;

        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            if (!Enum.TryParse<ReportTemplate>(templateKey, true, out var key))
                key = ReportTemplate.None;

            var content = TemplateProvider.GetTemplate(key);
            var projectItem = new ReportingRazorProjectItem(templateKey, content);

            return Task.FromResult<RazorLightProjectItem>(projectItem);
        }

        private readonly static Task<IEnumerable<RazorLightProjectItem>> _emptyItems = Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
    }
}

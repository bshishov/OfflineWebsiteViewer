using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DotLiquid;

namespace OfflineWebsiteViewer.Utility
{
    class LangTag : DotLiquid.Tag
    {
        private string _resourceName;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            _resourceName = markup;
        }

        public override void Render(Context context, TextWriter result)
        {
            result.Write(Resources.Language.ResourceManager.GetString(_resourceName.Trim()));
        }
    }
}

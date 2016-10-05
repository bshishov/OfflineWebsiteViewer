using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotLiquid;
using OfflineWebsiteViewer.Utility;

namespace OfflineWebsiteViewer
{
    static class TemplateRenderer
    {
        public static string NamespacePrefix = "OfflineWebsiteViewer.Resources.Templates.";

        static TemplateRenderer()
        {
            Template.RegisterTag<LangTag>("lang");
        }

        public static string RenderFromResource<T>(string resourcePath, T context)
        {
            //RegisterSafeTypeWithAllProperties(typeof(T));

            using (var stream = GetResource(resourcePath))
            {
                if (stream != null)
                {
                    var template = Template.Parse(new StreamReader(stream).ReadToEnd());
                    return template.Render(Hash.FromAnonymousObject(context));
                }
            }

            return "Stream in null";
        }

        public static string RenderFromResource(string resourcePath)
        {
            using (var stream = GetResource(resourcePath))
            {
                if (stream != null)
                {
                    var template = Template.Parse(new StreamReader(stream).ReadToEnd());
                    return template.Render();
                }
            }

            return "Stream in null";
        }

        public static void RegisterViewModel(Type rootType)
        {
            rootType
               .Assembly
               .GetTypes()
               .Where(t => t.Namespace == rootType.Namespace)
               .ToList()
               .ForEach(RegisterSafeTypeWithAllProperties);
        }

        public static void RegisterSafeTypeWithAllProperties(Type type)
        {
            Template.RegisterSafeType(type,
               type
               .GetProperties()
               .Select(p => p.Name)
               .ToArray());
        }

        private static Stream GetResource(string resourcePath)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(NamespacePrefix + resourcePath);
        }
    }
}

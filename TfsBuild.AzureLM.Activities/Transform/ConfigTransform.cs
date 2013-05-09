using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Web.XmlTransform;

namespace TfsBuild.AzureLM.Activities.Transform
{
    public static class ConfigTransform
    {
        public static string GetFilePathResolution(string source, string sourceRootPath)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException("source");

            if (Path.IsPathRooted(source) || string.IsNullOrWhiteSpace(sourceRootPath))
            {
                return source;
            }
            return Path.Combine(sourceRootPath, source);
        }


        public static bool DoTransform(string sourceFile, string transformFile, string destinationFile)
        {
            bool flag;

            if (string.IsNullOrWhiteSpace(sourceFile)) throw new ArgumentNullException("sourceFile");
            if (string.IsNullOrWhiteSpace(transformFile)) throw new ArgumentNullException("transformFile");
            if (string.IsNullOrWhiteSpace(destinationFile)) throw new ArgumentNullException("destinationFile");

            XmlTransformation xmlTransformation = null;
            XmlTransformableDocument xmlTransformableDocument = null;

            try
            {
                xmlTransformableDocument = OpenSourceFile(sourceFile);

                xmlTransformation = OpenTransformFile(transformFile);

                flag = xmlTransformation.Apply(xmlTransformableDocument);

                if (flag)
                {
                    SaveTransformedFile(xmlTransformableDocument, destinationFile);
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            finally
            {
                if (xmlTransformation != null)
                {
                    xmlTransformation.Dispose();
                }
                if (xmlTransformableDocument != null)
                {
                    xmlTransformableDocument.Dispose();
                }
            }
            return flag;
        }


        private static XmlTransformableDocument OpenSourceFile(string sourceFile)
        {
            XmlTransformableDocument xmlTransformableDocument;
            try
            {
                xmlTransformableDocument = new XmlTransformableDocument { PreserveWhitespace = true };
                xmlTransformableDocument.Load(sourceFile);
            }
            catch (XmlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                var message = new object[] { exception.Message };
                throw new Exception(
                    string.Format(CultureInfo.CurrentCulture, "Source Load Failed", message),
                    exception);
            }
            return xmlTransformableDocument;
        }

        private static XmlTransformation OpenTransformFile(string transformFile)
        {
            XmlTransformation xmlTransformation;
            try
            {
                xmlTransformation = new XmlTransformation(transformFile);
            }
            catch (XmlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                var message = new object[] { exception.Message };
                throw new Exception(string.Format(CultureInfo.CurrentCulture, "Transform Load Failed", message), exception);
            }
            return xmlTransformation;
        }

        private static void SaveTransformedFile(XmlDocument document, string destinationFile)
        {
            try
            {
                document.Save(destinationFile);
            }
            catch (XmlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                var message = new object[] { exception.Message };

                throw new Exception(string.Format(CultureInfo.CurrentCulture, "Write to destination failed", message), exception);
            }
        }
    }
}

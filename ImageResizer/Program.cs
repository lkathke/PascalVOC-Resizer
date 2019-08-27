using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommandLine;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => {
                    Loop(opts.InputDirectory, opts.OutputDirectory, opts.Width, opts.Height, opts.XMLDirectory, opts.RootDirectory);
                });
                
        }

        static void Loop(string inputPath, string outputPath, int width, int height, string xmlDirectory =  "", string rootDirectoryName = "")
        {
            var allFiles = System.IO.Directory.GetFiles(inputPath, "*.jpg");

            int fileCounter = 0;
            foreach (var file in allFiles)
            {
                ImageTuple tmpImage = new ImageTuple(file);
                if(!string.IsNullOrEmpty(xmlDirectory))
                {
                    var xmlFilePath = System.IO.Path.Combine(xmlDirectory, System.IO.Path.GetFileNameWithoutExtension(tmpImage.ImagePath) + ".xml");
                    if(System.IO.File.Exists(xmlFilePath))
                    {
                        tmpImage.XMLPath = xmlFilePath;
                    }
                }


                if (tmpImage.isValid == false)
                {
                    Console.WriteLine("Not valid: " + tmpImage);
                }
                else
                {
                    tmpImage.Resize(outputPath, width, height < 0 ? width : height, rootDirectoryName);
                }

                fileCounter++;
                Console.WriteLine($"Image {fileCounter}/{allFiles.Length} complete");
            }
            Console.WriteLine("Done");
        }
    }
    

    public class ImageTuple
    {
        public ImageTuple(string ImagePath, string XMLPath)
        {
            this.ImagePath = ImagePath;
            this.XMLPath = XMLPath;
        }

        public ImageTuple(string ImagePath)
        {
            var imageName = System.IO.Path.GetFileNameWithoutExtension(ImagePath);
            var imagePath = System.IO.Path.GetDirectoryName(ImagePath);
            var xmlPath = System.IO.Path.Combine(imagePath, imageName + ".xml");
            if (System.IO.File.Exists(xmlPath))
            {
                this.XMLPath = xmlPath;
            }
            this.ImagePath = ImagePath;
        }

        public string ImagePath;
        public string XMLPath;
        public bool isValid { get
            {
                return (!string.IsNullOrEmpty(ImagePath)) && (!string.IsNullOrEmpty(XMLPath));
            } }

        public void Resize(string outputPath, int newX_size, int height, string rootDirectoryName = "")
        {
            Image<Bgr, Byte> img1 = new Emgu.CV.Image<Bgr, Byte>(this.ImagePath);

            var oldX = img1.Width;
            var oldY = img1.Height;

            float ratio = (float) oldX / (float) newX_size;
            int newY = (int)Math.Round((double)((float)oldY / (float)ratio));
            int newX =  (int) Math.Round((double) ((float) oldX / (float) ratio));

            img1 = img1.Resize(newX, newY, Emgu.CV.CvEnum.Inter.LinearExact);

            var delta_w = newX_size - newX;
            var delta_h = height - newY;

            var top = delta_h / 2;
            var bottom = delta_h - top;

            var left = delta_w / 2;
            var right = delta_w - left;

            //img1.Save(@"C:\Users\lkathke\Desktop\EmguTest\resized.jpg");

            Mat newImage = new Mat();
            CvInvoke.CopyMakeBorder(img1, newImage, top, bottom, left, right, Emgu.CV.CvEnum.BorderType.Constant);

            newImage.Save(System.IO.Path.Combine(outputPath, System.IO.Path.GetFileName(this.ImagePath)));
            ResizeAnnotations(this.XMLPath, newX_size, height, newX, newY, oldX, oldY, top, left, outputPath, rootDirectoryName);

        }

           
        private void ResizeAnnotations(string xmlFile, int paddedImgWidth, int paddedImgHeight, int realWidth, int realHeight, int originalWidth, int originalHeight, int top, int left, string outputPath, string rootDirectoryName = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);

            var width = doc.DocumentElement.SelectSingleNode("/annotation/size/width");
            width.InnerXml = paddedImgWidth.ToString();

            var height = doc.DocumentElement.SelectSingleNode("/annotation/size/height");
            height.InnerXml = paddedImgHeight.ToString();

            var path = doc.DocumentElement.SelectSingleNode("/annotation/path");
            path.InnerText = (string.IsNullOrEmpty(rootDirectoryName) ? "" : rootDirectoryName + "/") + path.InnerText.Substring(path.InnerText.IndexOf("/") + 1);

            var objects = doc.DocumentElement.SelectNodes("/annotation/object");
            foreach(XmlNode obj in objects)
            {
                var name = obj.SelectSingleNode("name");
                var boundingBox = obj.SelectSingleNode("bndbox");

                var xmin_node = boundingBox.SelectSingleNode("xmin");
                var ymin_node = boundingBox.SelectSingleNode("ymin");
                var xmax_node = boundingBox.SelectSingleNode("xmax");
                var ymax_node = boundingBox.SelectSingleNode("ymax");


                float widthRatio = (float)realWidth / (float) originalWidth;
                float heightRatio = (float)realHeight / (float)originalHeight;

                var xmin = float.Parse(xmin_node.InnerText, CultureInfo.InvariantCulture.NumberFormat);
                var ymin = float.Parse(ymin_node.InnerText, CultureInfo.InvariantCulture.NumberFormat);
                var xmax = float.Parse(xmax_node.InnerText, CultureInfo.InvariantCulture.NumberFormat);
                var ymax = float.Parse(ymax_node.InnerText, CultureInfo.InvariantCulture.NumberFormat);

                float new_xmin = (xmin * widthRatio) + left;
                float new_xmax = (xmax * widthRatio) + left;
                float new_ymin = (ymin * heightRatio) + top;
                float new_ymax = (ymax * heightRatio) + top;


                xmin_node.InnerText = new_xmin.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                xmax_node.InnerText = new_xmax.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                ymin_node.InnerText = new_ymin.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                ymax_node.InnerText = new_ymax.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

                //CvInvoke.Rectangle(image, new System.Drawing.Rectangle((int)new_xmin, (int)new_ymin, (int)(new_xmax - new_xmin), (int)(new_ymax - new_ymin)), new Bgr(Color.Red).MCvScalar, 2);
            }
     
            doc.Save(System.IO.Path.Combine(outputPath, System.IO.Path.GetFileName(xmlFile)));
            
        }



        public override string ToString()
        {
            return $"{ImagePath} - {XMLPath}";
        }
    }


    class Options
    {
        [Option('i', HelpText = "The input directory (jpg and xml files are in there)", Required =true)]
        public string InputDirectory { get; set; }

        [Option('x', HelpText = "XML input directory (optional, if not set, using the same directory for images)", Required = false, Default = "")]
        public string XMLDirectory { get; set; }

        [Option('o', HelpText = "The output directory", Required = true)]
        public string OutputDirectory { get; set; }

        [Option('w', HelpText = "The Image width", Required = true)]
        public int Width { get; set; }

        [Option('h', HelpText = "The Image height", Required = false, Default = -1)]
        public int Height { get; set; }

        [Option('r', HelpText = "The root directory to write in the xml file", Required = false, Default = "")]
        public string RootDirectory { get; set; }

    }
}

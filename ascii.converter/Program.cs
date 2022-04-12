using OpenCvSharp;

namespace ascii.converter
{
    class Program
    {
        public static int CalcTermWidth(int termWidth, int termHeight, int imageWidth, int imageHeight)
        {
            double imageAttitude = (double)imageWidth / imageHeight;
            double termAttitude = (double)termWidth / termHeight;
            int newWidth;

            if (termWidth / imageAttitude < termHeight)
            {
                newWidth = termWidth;
            } else
            {
                newWidth = (int)(termHeight * termAttitude);
            }

            return newWidth;
        }

        public static Mat ResizeImage(Mat image, double? newWidth = null, double? newHeight = null)
        {
            double pixelAspect = 11.0f / 24.0f;
            double newScale;

            if (newWidth != null)
            {
                newScale = (double)newWidth / image.Width;
            }
            else if (newHeight != null)
            {
                newScale = (double)newHeight / image.Height;
            }
            else
            {
                return image;
            }

            Mat resizedImage = image.Resize(default, newScale, newScale * pixelAspect, InterpolationFlags.Area);

            return resizedImage;
        }

        public static double GetBrightness(int r, int g, int b)
        {
            double brightness = ((0.21 * r) + (0.72 * g) + (0.07 * b)) / 255;

            return brightness;
        }

        public static string AsciiImage(Mat image)
        {
            string asciiString = " .'`^\",:;Il!i ><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";

            var indexer = image.GetGenericIndexer<Vec3b>();
            string result = "";


            for (int y = 0; y < image.Height; y++)
            {

                for (int x = 0; x < image.Width; x++)
                {
                    Vec3b color = indexer[y, x];
                    byte temp = color.Item0;
                    color.Item0 = color.Item2;
                    color.Item2 = temp;
                    indexer[y, x] = color;

                    int symbolIndex = (int)(asciiString.Length * GetBrightness(color[0], color[1], color[2]));

                    result += asciiString[symbolIndex];
                }

                result += "\n";
            }

            return result;
        }

        static void Main(string[] args)
        {
            string filePath;

            if (args.Length == 0)
            {
                Console.Write("Input video location >> ");
                filePath = Console.ReadLine();
            } else
            {
                filePath = args[0];
            }

            if (!File.Exists(filePath)) throw new InvalidOperationException("Path is invalid!");
            Console.Clear();

            VideoCapture capture = new VideoCapture(filePath);
            Mat image = new Mat();

            double frameLength = 1 / capture.Fps;

            int originalCursorLeft = Console.CursorLeft;
            int originalCursorTop = Console.CursorTop;
            int previousImageWidth = 0;

            while (capture.IsOpened())
            {
                DateTime startTime = DateTime.Now;

                capture.Read(image);
                if (image.Empty()) break;

                int renderWidth = CalcTermWidth(Console.WindowWidth, Console.WindowHeight, image.Width, image.Height);
                string asciiImage = AsciiImage(ResizeImage(image, renderWidth));

                while ((DateTime.Now - startTime).TotalSeconds < frameLength)
                {
                    Thread.Sleep(0);
                }

                if (previousImageWidth != renderWidth)
                {
                    previousImageWidth = renderWidth;

                    Console.Clear();
                }

                Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
                Console.WriteLine(asciiImage);
            }
        }
    }
}

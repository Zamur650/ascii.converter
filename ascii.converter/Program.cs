using OpenCvSharp;

namespace ascii.converter
{
  class Program
  {
    public static Mat ResizeImageByWidth(Mat image, double newWidth)
    {
      double pixelAspect = 11.0f / 24.0f;
      double newScale = (double)newWidth / image.Width;

      Mat resizedImage = image.Resize(default, newScale, newScale * pixelAspect, InterpolationFlags.Area);

      return resizedImage;
    }

    public static Mat ResizeImageByHeight(Mat image, double newHeight)
    {
      double pixelAspect = 24.0f / 11.0f;
      double newScale = (double)newHeight / image.Height;

      Mat resizedImage = image.Resize(default, newScale * pixelAspect, newScale, InterpolationFlags.Area);

      return resizedImage;
    }

    public static double GetBrightness(int r, int g, int b)
    {
      double brightness = ((0.21 * r) + (0.72 * g) + (0.07 * b)) / 255;

      return brightness;
    }

    public static string AsciiImage(Mat image)
    {
      string asciiString = " .,:;+*?%S#@";

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
      string? filePath;

      if (args.Length == 0)
      {
        Console.Write("Input video location >> ");
        filePath = Console.ReadLine();
      }
      else
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
      int previousImageHeight = 0;

      while (capture.IsOpened())
      {
        DateTime startTime = DateTime.Now;

        capture.Read(image);
        if (image.Empty()) break;

        double imageAttitude = (double)image.Width / image.Height;
        string asciiImage;

        if (imageAttitude > 1)
        {
          asciiImage = AsciiImage(ResizeImageByWidth(image, Console.WindowWidth));

          if (previousImageWidth != Console.WindowWidth)
          {
            previousImageWidth = Console.WindowWidth;

            Console.Clear();
          }
        }
        else
        {
          asciiImage = AsciiImage(ResizeImageByHeight(image, Console.WindowHeight));

          if (previousImageHeight != Console.WindowHeight)
          {
            previousImageHeight = Console.WindowHeight;

            Console.Clear();
          }
        }

        while ((DateTime.Now - startTime).TotalSeconds < frameLength)
        {
          Thread.Sleep(0);
        }

        Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
        Console.WriteLine(asciiImage);
      }
    }
  }
}

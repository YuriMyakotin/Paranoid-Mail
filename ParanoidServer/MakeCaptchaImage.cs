using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Paranoid
{
	public static class HumanFriendlyInteger
	{
		static readonly string[] ones = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
		static readonly string[] teens = new string[] { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
		static readonly string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
		static readonly string[] thousandsGroups = { "", " Thousand", " Million", " Billion" };

		private static string FriendlyInteger(int n, string leftDigits, int thousands)
		{
			if (n == 0)
			{
				return leftDigits;
			}
			string friendlyInt = leftDigits;
			if (friendlyInt.Length > 0)
			{
				friendlyInt += " ";
			}

			if (n < 10)
			{
				friendlyInt += ones[n];
			}
			else if (n < 20)
			{
				friendlyInt += teens[n - 10];
			}
			else if (n < 100)
			{
				friendlyInt += FriendlyInteger(n % 10, tens[n / 10 - 2], 0);
			}
			else if (n < 1000)
			{
				friendlyInt += FriendlyInteger(n % 100, (ones[n / 100] + " Hundred"), 0);
			}
			else
			{
				friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
			}

			return friendlyInt + thousandsGroups[thousands];
		}

		public static string IntegerToWritten(int n)
		{
			if (n == 0)
			{
				return "Zero";
			}
			else if (n < 0)
			{
				return "Negative " + IntegerToWritten(-n);
			}

			return FriendlyInteger(n, "", 0);
		}

	}
	public static class MakeCaptchaString
	{
		public static string CaptchaStr(int A, int B, bool isPlus)
		{
			string PlusMinus = isPlus ? " + " : " - ";
			return HumanFriendlyInteger.IntegerToWritten(A) + PlusMinus + HumanFriendlyInteger.IntegerToWritten(B);
		}

	}

	public static class MakeCaptchaImage
	{
		public static byte[] MakeCaptcha(String sCaptchaText)
		{
			const int iHeight = 96;
			const int iWidth = 640;
			Random oRandom = new Random();

			//int[] aBackgroundNoiseColor = new int[] { 150, 150, 150 };
			//int[] aTextColor = new int[] { 0, 0, 0 };
			int[] aFontEmSizes = new int[] { 20, 25, 30, 35 };

			string[] aFontNames = new string[]
{
 "Comic Sans MS",
 "Arial",
 "Times New Roman",
 "Georgia",
 "Verdana",
 "Geneva"
};
	FontStyle[] aFontStyles = {
		FontStyle.Bold,
		FontStyle.Italic,
		FontStyle.Regular,
		FontStyle.Strikeout,
	};

			HatchStyle[] aHatchStyles = {
 HatchStyle.BackwardDiagonal, HatchStyle.Cross,
	HatchStyle.DashedDownwardDiagonal, HatchStyle.DashedHorizontal,
 HatchStyle.DashedUpwardDiagonal, HatchStyle.DashedVertical,
	HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross,
 HatchStyle.Divot, HatchStyle.DottedDiamond, HatchStyle.DottedGrid,
	HatchStyle.ForwardDiagonal, HatchStyle.Horizontal,
 HatchStyle.HorizontalBrick, HatchStyle.LargeCheckerBoard,
	HatchStyle.LargeConfetti, HatchStyle.LargeGrid,
 HatchStyle.LightDownwardDiagonal, HatchStyle.LightHorizontal,
	HatchStyle.LightUpwardDiagonal, HatchStyle.LightVertical,
 HatchStyle.Max, HatchStyle.Min, HatchStyle.NarrowHorizontal,
	HatchStyle.NarrowVertical, HatchStyle.OutlinedDiamond,
 HatchStyle.Plaid, HatchStyle.Shingle, HatchStyle.SmallCheckerBoard,
	HatchStyle.SmallConfetti, HatchStyle.SmallGrid,
 HatchStyle.SolidDiamond, HatchStyle.Sphere, HatchStyle.Trellis,
	HatchStyle.Vertical, HatchStyle.Wave, HatchStyle.Weave,
 HatchStyle.WideDownwardDiagonal, HatchStyle.WideUpwardDiagonal, HatchStyle.ZigZag
};


			//Creates an output Bitmap
			//Bitmap oOutputBitmap = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
			//Graphics oGraphics = Graphics.FromImage(oOutputBitmap);
			//oGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

			//Create a Drawing area

			Bitmap oTmpBitmap = new Bitmap(iWidth/2, iHeight/2, PixelFormat.Format24bppRgb);
			Graphics oTmpGraphics = Graphics.FromImage(oTmpBitmap);

			RectangleF oRectangleF = new RectangleF(0, 0, iWidth/2, iHeight/2);
			Brush oBrush = default(Brush);

			//Draw background (Lighter colors RGB 100 to 255)
			oBrush = new HatchBrush(aHatchStyles[oRandom.Next
				(aHatchStyles.Length - 1)], Color.FromArgb((oRandom.Next(80,140)),
				(oRandom.Next(80, 140)), (oRandom.Next(80, 140))), Color.FromArgb((oRandom.Next(140, 255)),
				(oRandom.Next(140, 255)), (oRandom.Next(140, 255))));

			oTmpGraphics.FillRectangle(oBrush, oRectangleF);

			Bitmap oOutputBitmap = new Bitmap(oTmpBitmap, iWidth, iHeight);
			Graphics oGraphics = Graphics.FromImage(oOutputBitmap);
			oGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;


			Matrix oMatrix = new Matrix();

			for (int i = 0; i < sCaptchaText.Length; i++)
			{
				oMatrix.Reset();
				int iChars = sCaptchaText.Length;
				int x = 8+(iWidth-8) / (iChars + 1) * i;
				int y = iHeight / 2 + oRandom.Next(-40,-10);

				//Rotate text Random
				oMatrix.RotateAt(oRandom.Next(-15, 15), new PointF(x, y));
				oGraphics.Transform = oMatrix;

				//Draw the letters with Random Font Type, Size and Color
				oGraphics.DrawString
				(
					//Text
				sCaptchaText.Substring(i, 1),
					//Random Font Name and Style
				new Font(aFontNames[oRandom.Next(aFontNames.Length - 1)],
				   aFontEmSizes[oRandom.Next(aFontEmSizes.Length - 1)],
				   aFontStyles[oRandom.Next(aFontStyles.Length - 1)]),
					//Random Color (Darker colors RGB 0 to 100)
				new SolidBrush(Color.FromArgb(oRandom.Next(0, 100),
				   oRandom.Next(0, 100), oRandom.Next(0, 100))),
				x,
				oRandom.Next(15, 30)
				);
				oGraphics.ResetTransform();
			}

			for(int i = 0; i <12; i++)
			{
				Pen P1 = new Pen(Color.FromArgb(oRandom.Next(0, 255), oRandom.Next(0, 255), oRandom.Next(0, 255)), oRandom.Next(1, 2));

				oGraphics.DrawLine(P1, oRandom.Next(0, 30), oRandom.Next(1, iHeight), oRandom.Next(iWidth-30, iWidth), oRandom.Next(1, 96));
			}


			Bitmap BlurredBitmap = Blur(oOutputBitmap, 2);

			MemoryStream oMemoryStream = new MemoryStream();
			BlurredBitmap.Save(oMemoryStream, ImageFormat.Png);
			byte[] oBytes = oMemoryStream.GetBuffer();


			oMemoryStream.Close();
			return oBytes;

		}


		private static Bitmap Blur(Bitmap image, int blurSize)
		{
			Bitmap blurred = new Bitmap(image.Width, image.Height);

			// make an exact copy of the bitmap provided
			using (Graphics graphics = Graphics.FromImage(blurred))
				graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
					new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

			// look at every pixel in the blur rectangle
			for (int xx = 0; xx < image.Width; xx++)
			{
				for (int yy = 0; yy < image.Height; yy++)
				{
					int avgR = 0, avgG = 0, avgB = 0;
					int blurPixelCount = 0;

					// average the color of the red, green and blue for each pixel in the
					// blur size while making sure you don't go outside the image bounds
					for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
					{
						for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
						{
							Color pixel = blurred.GetPixel(x, y);

							avgR += pixel.R;
							avgG += pixel.G;
							avgB += pixel.B;

							blurPixelCount++;
						}
					}

					avgR = avgR / blurPixelCount;
					avgG = avgG / blurPixelCount;
					avgB = avgB / blurPixelCount;

					// now that we know the average for the blur size, set each pixel to that color
					for (int x = xx; x < xx + blurSize && x < image.Width ; x++)
						for (int y = yy; y < yy + blurSize && y < image.Height ; y++)
							blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
				}
			}

			return blurred;
		}

	}
}

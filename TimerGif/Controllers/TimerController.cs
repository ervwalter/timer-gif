using AttributeRouting.Web.Mvc;
using GetFeedback;
using GifComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace TimerImage.Controllers
{
	public class TimerController : Controller
	{
		public static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
		public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public const string BackgroundPath = "~/content/background.png";
		public const int ImageWidth = 500;
		public const int ImageHeight = 96;
		public const int ImageX = 0;
		public const int ImageY = 0;
		public const string FontName = "Calibri";
		public const float FontSize = 72f;
		public const bool FontBold = true;

		[GET("/")]
		public ActionResult Index()
		{
			return View();
		}

		[OutputCache(Duration = 1, Location = System.Web.UI.OutputCacheLocation.Server)]
		[CustomContentType("image/gif")]
		[Compress]
		[GET("/timer/{timestamp?}")]
		public ActionResult Timer(long? timestamp)
		{
			try
			{
				byte[] output = null;
				using (MemoryStream stream = new MemoryStream())
				{
					int repeatCount = 0; //repeat forever
					DateTime end = Epoch.AddSeconds(timestamp ?? 0);
					TimeSpan remaining = end - DateTime.UtcNow;
					if (remaining.TotalSeconds < 0)
					{
						remaining = TimeSpan.FromSeconds(0);
					}

					if (remaining.TotalSeconds <= 60)
					{
						repeatCount = -1; //don't repeat
					}

					using (Image background = Image.FromFile(Server.MapPath(BackgroundPath)))
					{
						using (Font font = new Font(FontName, FontSize, FontBold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel))
						{
							using (var disposer = new Disposer())
							{
								var target = new SynchronizeInvokeStub();
								AnimatedGifEncoder encoder = new AnimatedGifEncoder(target);
								encoder.RepeatCount = repeatCount;
								encoder.OutputStream = stream;
								encoder.QuantizerType = GifComponents.Quantizing.QuantizerType.Octree;
								{

									int count = 0;
									while (remaining.TotalSeconds >= 0 && count < 60)
									{
										Bitmap bitmap = disposer.Track(new Bitmap(background));
										using (Graphics g = Graphics.FromImage(bitmap))
										{
											StringFormat format = new StringFormat();
											format.Alignment = StringAlignment.Center;
											format.LineAlignment = StringAlignment.Center;
											g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
											string days, hours, minutes, seconds;
											if (remaining.Days > 99)
											{
												days = "--";
												hours = "--";
												minutes = "--";
												seconds = "--";
												count = 99; //causes the loop to end after this one
											}
											else
											{
												days = remaining.Days.ToString("00");
												hours = remaining.Hours.ToString("00");
												minutes = remaining.Minutes.ToString("00");
												seconds = remaining.Seconds.ToString("00");
											}
											g.DrawString(days, font, Brushes.White, new RectangleF(ImageX, ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(hours, font, Brushes.White, new RectangleF(ImageX + (ImageWidth / 4), ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(minutes, font, Brushes.White, new RectangleF(ImageX + (2 * ImageWidth / 4), ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(seconds, font, Brushes.White, new RectangleF(ImageX + (3 * ImageWidth / 4), ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(":", font, Brushes.White, new RectangleF(ImageX + (ImageWidth / 4) - ImageWidth / 8, ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(":", font, Brushes.White, new RectangleF(ImageX + (2 * ImageWidth / 4) - ImageWidth / 8, ImageY, ImageWidth / 4, ImageHeight), format);
											g.DrawString(":", font, Brushes.White, new RectangleF(ImageX + (3 * ImageWidth / 4) - ImageWidth / 8, ImageY, ImageWidth / 4, ImageHeight), format);
										}

										var frame = new GifFrame(bitmap);
										frame.Delay = 100;
										encoder.AddFrame(frame);

										count++;
										remaining = remaining.Subtract(OneSecond);
									}
								}
								encoder.Start();
								encoder.WaitUntilDone();
							}
						}
					}
					output = stream.ToArray();
				}
				return new FileContentResult(output, "image/gif");
			}
			catch
			{
				return new FilePathResult(Server.MapPath(BackgroundPath), "image/gif");
			}
		}
	}
}

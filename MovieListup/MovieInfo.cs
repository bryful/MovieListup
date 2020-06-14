using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MovieListup
{
	public enum MOVIE_TYPE
	{
		QT = 0,
		MP4,
		AVI,
		OTHER
	}


	public class MovieInfo
	{
		private MOVIE_TYPE m_MOVIE_TYPE = MOVIE_TYPE.OTHER;
		public MOVIE_TYPE MOVIE_TYPE {  get { return m_MOVIE_TYPE; } }
		private string m_FilePath = "";
		public string FilePath {  get { return m_FilePath; } }
		private string m_FileName = "";
		public string FileName
		{
			get { return m_FileName; }
		}
		private int m_Width = 0;
		public int Width { get { return m_Width; } }
		private int m_Height = 0;
		public int Height { get { return m_Height; } }
		private double m_FrameRate = 0;
		public double FrameRate { get { return m_FrameRate; } }
		private double m_Duration = 0;
		public double Duration { get { return m_Duration; } }
		private int m_FrameCount = 0;
		public int FrameCount { get { return m_FrameCount; } }
		private long m_FileSize = 0;
		public long FileSize { get { return m_FileSize; } }
		public string Comment = "";
		private int m_BoldCount = 8;
		public int BoldCount
		{
			get { return m_BoldCount; }
			set
			{
				m_BoldCount = value;
				if (m_BoldCount < 0) m_BoldCount = 0;
			}
		}
		// ********************************************
		public MovieInfo(string p = "")
		{
			if (p != "") SetFilePath(p); else Init();
		}
		// ********************************************
		public void Init()
		{
			m_MOVIE_TYPE = MOVIE_TYPE.OTHER;
			m_FilePath = "";
			m_FileName = "";
			m_Width = 0;
			m_Height = 0;
			m_FrameRate = 0;
			m_Duration = 0;
			m_FileSize = 0;
			Comment = "";
		}
		// ********************************************
		public enum DATAPOS
		{
			/// <summary>
			/// ファイルの先頭
			/// </summary>
			TOP =0,
			/// <summary>
			/// ファイルの末尾
			/// </summary>
			END
		}
		// ********************************************
		/// <summary>
		/// ファイルの先頭、或いは末尾を指定サイズ読み込む
		/// </summary>
		/// <param name="p">ファイルのパス</param>
		/// <param name="sz">読み込むサイズ</param>
		/// <param name="dp">読み込み位置</param>
		/// <returns>読み込んだbyte配列</returns>
		private byte [] GetByteData(string p ,long sz,DATAPOS dp = DATAPOS.TOP)
		{
			byte[] ret = new byte[0];
			if (File.Exists(p) == false) return ret;
			FileStream fs = new System.IO.FileStream(p,FileMode.Open,FileAccess.Read);
			//ファイルサイズと読み込みサイズのチェック。
			if (sz > fs.Length) sz = fs.Length;

			ret = new byte[sz];
			int rs = 0;
			try
			{
				if (dp == DATAPOS.TOP)
				{
					fs.Seek(0, SeekOrigin.Begin);
				}
				else
				{
					fs.Seek(-sz, SeekOrigin.End);
				}
				rs = fs.Read(ret, 0, (int)sz);
				
			}
			finally
			{
				fs.Close();
			}
			//読み込みサイズでエラーチェック
			if (rs != sz)
			{
				
				ret = new byte[0];
			}
			return ret;
		}
		// ********************************************
		/// <summary>
		/// していたバイト配列から文字列を探す
		/// </summary>
		/// <param name="h">対象のbyte配列</param>
		/// <param name="tag">探す文字列</param>
		/// <returns>見つけた位置、見つからなかったら-1</returns>
		private long FindByteData(byte [] h,string tag)
		{
			long ret = -1;
			if (h.Length <= 0) return ret;
			if (tag.Length <= 0) return ret;
			byte[] tagB = System.Text.Encoding.ASCII.GetBytes(tag);
			long v = tagB.Length;
			if (v > 4) v = 4;
			long cnt = h.Length - v;
			for ( long i = 0; i<cnt; i++)
			{
				bool b = true;
				for ( long j=0; j < v;j++)
				{
					if( h[i+j] != tagB[j])
					{
						b = false;
						break;
					}
				}
				if(b==true)
				{
					ret = i;
					break;
				}
			}

			return ret;
		}
		// ********************************************
		/// <summary>
		/// ファイルのヘッダーを読んでムービーファイルの種類を識別
		/// </summary>
		/// <param name="p">ムービーファイルのパス</param>
		/// <returns>ムービーの種類</returns>
		private MOVIE_TYPE GetHeader(string p)
		{
			 MOVIE_TYPE ret = MOVIE_TYPE.OTHER;

			byte[] bs = GetByteData(p, 16, 0);
			if (bs.Length <= 0) return ret;

			if ((bs[0]==0x52)&&(bs[1]==0x49)&&(bs[2]==0x46)&&(bs[3]==0x46) // 52 49 46 46
				&&(bs[8]==0x41)&&(bs[9]==0x56)&&(bs[10]==0x49)&&(bs[11]==0x20) ) //41 56 49 20
			{
				ret = MOVIE_TYPE.AVI;
			}
			else if ( (bs[4]==0x66)&&(bs[5]==0x74)&&(bs[6]==0x79)&&(bs[7]==0x70))//66 74 79 70
			{
				if( (bs[8]==0x71)&&(bs[9]==0x74)) //71 74 20 20
				{
					ret = MOVIE_TYPE.QT;
				}else if( (bs[8]==0x6D)&&(bs[9]==0x70)&&(bs[10]==0x34)) //6D 70 34
				{
					ret = MOVIE_TYPE.MP4;
				}
			}
			return ret;
		}
		// ********************************************
		/// <summary>
		/// ムービーファイルを読み込んでいろいろ調べる
		/// </summary>
		/// <param name="p">ムービーのパス</param>
		/// <returns>対象のムービーだったらtrue</returns>
		public bool SetFilePath(string p)
		{
			bool ret = false;
			Init();
			if (File.Exists(p) == false) return ret;
			m_MOVIE_TYPE = GetHeader(p);
			if (m_MOVIE_TYPE == MOVIE_TYPE.OTHER) return ret;
			m_FilePath = p;
			m_FileName = Path.GetFileName(p);
			FileInfo finfo = new FileInfo(p);
			m_FileSize = finfo.Length;

			switch (m_MOVIE_TYPE)
			{
				case MOVIE_TYPE.AVI:
					ret =GetStatusAVI();
					break;
				case MOVIE_TYPE.QT:
				case MOVIE_TYPE.MP4:
					ret = GetStatusQT();
					break;
			}
			if(ret== false)
			{
				Init();
			}
			return ret;
		}
		// ********************************************
		/// <summary>
		/// m_FilePathのAVIファイルの情報を獲得
		/// </summary>
		/// <returns></returns>
		private bool GetStatusAVI()
		{
			bool ret = false;
			byte[] h = GetByteData(m_FilePath, 0x50, 0);
			if (h.Length <= 0) return ret;
			m_Width         = ((int)h[0x40]) + ((int)h[0x41] << 8) + ((int)h[0x42] << 16) + (int)(h[0x43] << 24);
			m_Height        = ((int)h[0x44]) + ((int)h[0x45] << 8) + ((int)h[0x46] << 16) + ((int)h[0x47] << 24);
			m_FrameCount    = (h[0x30]) + (h[0x31] << 8) + (h[0x32] << 16) + (h[0x33] << 24);
			int fri         = (h[0x20]) + (h[0x21] << 8) + (h[0x22] << 16) + (h[0x23] << 24);
			if (fri>0) {
				double fr = 1000000.0 / (double)fri;

				int tmp = (int)(fr * 1000 + 0.5);
				fr  = (double)tmp / 1000;
				m_FrameRate     = fr;
			}
			else
			{
				return ret;
			}
			int frv = (int)(m_FrameRate + 0.5);
			m_Duration = (double)m_FrameCount /(double)frv;
			ret = true;
			return ret;
		}
		// ********************************************
		// ********************************************
		/// <summary>
		/// m_FilePathのQuickTime/mp4ファイルの情報を獲得
		/// </summary>
		/// <returns></returns>
		private bool GetStatusQT()
		{
			bool ret = false;
			long sz = 1024 * 100;
			//まずファイル末を調べる

			byte[] h = GetByteData(m_FilePath, sz, DATAPOS.END);

			long idx = -1;
			idx = FindByteData(h, "moov");
			if (idx < 0)
			{
				//見つからなかった先頭を調べる
				h = GetByteData(m_FilePath, sz, DATAPOS.TOP);
				idx = FindByteData(h, "moov");
				if (idx < 0) return ret;
			}
			idx = FindByteData(h, "tkhd");

			if (idx >= 0)
			{
				m_Width = ((int)h[idx + 78] << 24) + ((int)h[idx + 79] << 16) + ((int)h[idx + 80] << 8) + (int)h[idx + 81];
				m_Height = ((int)h[idx + 82] << 24) + ((int)h[idx + 83] << 16) + ((int)h[idx + 84] << 8) + (int)h[idx + 85];
			}
			idx = FindByteData(h, "stts");
			if (idx >= 0)
			{
				m_FrameCount = ((int)h[idx + 12] << 24) + ((int)h[idx + 13] << 16) + ((int)h[idx + 14] << 8) + (int)h[idx + 15];
			}
			if (m_FrameCount > 0)
			{
				idx = FindByteData(h, "mvhd");
				if (idx >= 0)
				{
					int ts = ((int)h[idx + 16] << 24) + ((int)h[idx + 17] << 16) + ((int)h[idx + 18] << 8) + (int)h[idx + 19];
					int du = ((int)h[idx + 20] << 24) + ((int)h[idx + 21] << 16) + ((int)h[idx + 22] << 8) + (int)h[idx + 23];
					m_Duration = ((double)du / (double)ts);
					m_FrameRate = (double)m_FrameCount / (double)m_Duration;
					m_FrameRate = (double)((int)(m_FrameRate * 1000 + 0.5)) / 1000;


					if ((m_FrameRate == 24.0) || (m_FrameRate == 30.0) || (m_FrameRate == 29.97) || (m_FrameRate == 23.976))
					{

					}
					else if ((m_FrameRate > 23.96) && (m_FrameRate < 23.98))
					{
						m_FrameRate = 23.976;
					}else if((m_FrameRate > 23.99) && (m_FrameRate < 24.01))
					{
						m_FrameRate = 24;
					}else if((m_FrameRate > 29.96) && (m_FrameRate < 30.01))
					{
						m_FrameRate = 30;
					}


					//フレームれーとを四捨五入してからdurationを求める
					int frv = (int)(m_FrameRate + 0.5);
					m_Duration = (double)m_FrameCount / (double)frv;
				}
			}
			ret = true;
			return ret;
		}
		public string InfoStr()
		{
			return String.Format("Name:{0},Image Size:{1}x{2},FrameRate:{3},Duration:{4},FrameCount:{5},File Size:{6}",
				m_FileName,
				m_Width, m_Height,
				m_FrameRate,
				m_Duration,
				m_FrameCount,
				m_FileSize
				);
		}
		public string SizeInfoStr
		{
			get
			{
				return String.Format("{0}x{1}", m_Width, m_Height);
			}

		}
		public string FrameRateInfoStr
		{
			get
			{
				return String.Format("{0} fps", m_FrameRate);
			}

		}
		public string DurationInfoStr
		{
			get
			{
				int fps = (int)(m_FrameRate + 0.5);
				int frm = m_FrameCount - m_BoldCount;
				int sec = frm / fps;
				int koma  = m_FrameCount % fps;
				return String.Format("{0:D2}+{1:D2} ({2})", sec,koma,frm);
			}

		}
		public string FileSizeInfoStr
		{
			get
			{
				string ret = "";
				if (m_FileSize > 1024 * 1024 * 1024)
				{
					double gg = (double)m_FileSize / (1024 * 1024 * 1024);
					ret = String.Format("{0:#.0}Gbyte", gg);
				}
				else if (m_FileSize > 1024 * 1024)
				{
					double mm = (double)m_FileSize / (1024 * 1024);
					ret = String.Format("{0:#.0}Mbyte", mm);
				}
				else if (m_FileSize > 1024)
				{
					double kk = (double)m_FileSize / 1024;
					ret = String.Format("{0:#.0}Kbyte", kk);
				}
				else 
				{
					ret = String.Format("{0}Byte", m_FileSize);
				}

				return ret;
			}

		}
	}
}

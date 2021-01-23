using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Text.RegularExpressions;


namespace Tez_Kontrol
{   
    public partial class Form1 : Form
    {
        //Veri girişi yapılan pdf dökümanın aktarıldığı global değişken.
        public StringBuilder text = new StringBuilder();
        #region Veri Girişi İşlemleri
        public string veriOku()
        {
            openFileDialog1.ShowDialog();  //Dosya seçme menüsü açar         
            openFileDialog1.Filter = "Pdf Dosyası |*.pdf";//Dosyayı sadece pdf olarak filtreler
            return openFileDialog1.FileName; //Seçilen dosya ismini döndürür.
        }
        public string ReadPdfFile(string fileName)
        {
            //Giriş yapılan pdf dökümanı program için okunabilir hale getiriliyor.
            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();//Gelen döküman text değişkenine aktarılıyor.
        }
        #endregion
        public Form1()//Yapıcı metot
        {
            InitializeComponent();//Form komponentleri tanımlanıp, yükleniyor.
        }       

        #region Kaynak  İşlemleri
        //Burada Kaynak sekmesindeki butonların yaptığı işlemler yer almaktadır
        private void btnKaynakVeriOku_Click(object sender, EventArgs e)
        {
            text.Clear();//text dosyası temizleniyor.
            //Bu kod parçacığı kaynak sekmesindeki döküman textboxına veriyi çeker 
            //ve aynı zamanda text değişkenine veriyi atar
            rtbKaynakDokuman.Text = ReadPdfFile(veriOku());
        }     
        private void btnKaynakBul_Click(object sender, EventArgs e)
        {
            //Maksimum 100 kaynak belirlenmiştir.          
            rtbKaynakRapor.Clear();//Kaynak sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int kaynakSayisi = 0; //Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)
            {
                kaynakSayisi = Kaynakbul(i, text.ToString());//Kaynak bul metodundan gelen eşleşme sayısı değişkene atandı.
                if (kaynakSayisi >= 1)//Kaynak sayısı 1 'e eşit ve 1 'den büyük ise kaynak bulunmuştur.
                {
                    rtbKaynakRapor.Text += "["+i+"] numaralı kaynak en az 1 kere kullanılmıştır. \n";//Raporlama gerçekleştirilir.
                }
                else
                {
                    rtbKaynakRapor.Text += "[" + i + "] numaralı kaynak kullanılmamıştır.\n";//Kaynağın bulunmadığı rapor edilir.
                }
            }          
        }
        private void btnKaynakAtıfBul_Click(object sender, EventArgs e)
        {
            rtbKaynakRapor.Clear();//Kaynak sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)
            {
                kaynakSayisi = Kaynakbul(i, text.ToString());//Kaynak bul metodundan gelen eşleşme sayısı değişkene atandı.
                if (kaynakSayisi != 0 && kaynakSayisi != 1)//Kaynak sayısı 0 dan ve 1 den farklı ise kaynak bulunmuştur. 1 den farklı olmasının sebebi kaynağın sadece belirtildiği kaynaklar kısmında geçerli olmamasıdır.
                {
                    rtbKaynakRapor.Text += "[" + i + "] numaralı kaynağa " + (kaynakSayisi-1) + " kere atıf yapılmıştır.. \n";//Raporlama gerçekleştirilir.
                }
                else
                {
                    rtbKaynakRapor.Text += "[" + i + "] numaralı kaynağa atıf yapılmamıştır. Lütfen kaynağa atıf yapınız. \n";//Kaynağa atıf bulunmadığı rapor edilir.
                }
            }
        }
        private void btnKaynakBlokAtifBul_Click(object sender, EventArgs e)
        {
            //Burada kaynakların [2-4] vb. standartında kontrolü gerçekleştirilir.
            rtbKaynakRapor.Clear();//Kaynak sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            int k = 1;//Aranılan kaynak değeri
            for (int i = 1; i <= 100; i++)
            {                
                for (int j = 1; j <= 100; j++)
                {
                    kaynakSayisi = KaynakBlokAtifKoseli(i,j,text.ToString());//Kaynak bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (kaynakSayisi != 0 && k>=i && k<=j && Math.Abs(i-j) <= 2)//Burada iç içe for döngüsü ile aranılan kaynak değerin belirtilen aralıkta olup olmadığı kontrol edilir.
                    {
                        rtbKaynakRapor.Text += k +" numaralı kaynak blok atıfta [" + i + "-" + j +"]"+" geçiyor.  \n";
                    }
                    else if (kaynakSayisi != 0 && k >= i && k <= j && Math.Abs(i-j) > 2)//Belirtilen aralıkta hatalı kullanımı verilen kaynak raporlandı.
                    {
                        rtbKaynakRapor.Text += k + " numaralı kaynak blok atıfta [" + i + "-" + j + "]" + "hatalı geçiyor.  \n";
                    }
                }
                k++;
            }
            kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)//Burada aranılan kaynak değeri [3, şeklinde aranmaktadır.
            {
                kaynakSayisi = KaynakBlokAtifKoseBasi(i, text.ToString());//Kaynak bul metodundan gelen eşleşme sayısı değişkene atandı.
                if (kaynakSayisi != 0)
                {
                    rtbKaynakRapor.Text += i+" numaralı kaynak ["+i+", şeklinde "+kaynakSayisi+" defa geçiyor.\n";//Bulunan kaynak raporlandı.
                }
            }
            kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)//Burada belirtilen kaynak [rakam, aranılan değer, şeklinde aranmıştır.
            {
                kaynakSayisi = KaynakBlokAtifOrtaRakam(i, text.ToString());//Bulunan değerlerin atamasının yapıldığı değişkendir.
                if (kaynakSayisi != 0)
                {
                    rtbKaynakRapor.Text += i + " numaralı kaynak , " + i + ", şeklinde " + kaynakSayisi + " defa geçiyor.\n";//Raporlama gerçekleştirilir.
                }
            }
            kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)//Burada belirtilen kaynak [sayı, aranılan değer, şeklinde aranmıştır.
            {
                kaynakSayisi = KaynakBlokAtifOrtaSayi(i, text.ToString());// Bulunan değerlerin atamasının yapıldığı değişkendir.
                if (kaynakSayisi != 0)
                {
                    rtbKaynakRapor.Text += i + " numaralı kaynak [sayı, " + i + ", şeklinde " + kaynakSayisi + " defa geçiyor.\n";//Raporlama gerçekleştirilir.
                }
            }
            kaynakSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 100; i++)//Burada belirtilen kaynak , 3] şeklinde aranmıştır.
            {
                kaynakSayisi = KaynakBlokAtifKoseSonu(i, text.ToString());// Bulunan değerlerin atamasının yapıldığı değişkendir.
                if (kaynakSayisi != 0)
                {
                    rtbKaynakRapor.Text += i + " numaralı kaynak , " + i + "] şeklinde " + kaynakSayisi + " defa geçiyor.\n";//Raporlama gerçekleştirilir.
                }
            }
        }
        #endregion
        #region Kaynak Metot
        //Burada Kaynak sekmesindeki yapılan işlemlerin metotları bulunmaktadır.
        public static int Kaynakbul(int kaynak, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\[" + kaynak + "]"; //Aranmak istenilen regex ifadesi tanımlandı. [1] ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int KaynakBlokAtifKoseli(int kaynak,int kaynak2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\[" + kaynak + "–" + kaynak2 + "]"; //Aranmak istenilen regex ifadesi tanımlandı. [2–4] vb

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int KaynakBlokAtifKoseBasi(int kaynak, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\[" + kaynak +","; // Aranmak istenilen regex ifadesi tanımlandı. [3, vb.

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int KaynakBlokAtifOrtaRakam(int kaynak, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\[[0-9]\,." + kaynak + ","; //Aranmak istenilen regex ifadesi tanımlandı. [2, 7, 40] içerisinde [sayı, aranandeğer, vb.

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int KaynakBlokAtifOrtaSayi(int kaynak, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\[[0-9][0-9]\,." + kaynak + ","; //Aranmak istenilen regex ifadesi tanımlandı. [22, 7, 40] içerisinde [sayı, aranandeğer, vb.

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);//Eşleşme sayısı değişkeni tanımlandı.
            int matchCount = 0;
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int KaynakBlokAtifKoseSonu(int kaynak, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @",." + kaynak + "]"; //Aranmak istenilen regex ifadesi tanımlandı. , 3] vb.

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        #endregion

        #region Tablo İşlemleri
        //Burada Tablo sekmesindeki butonların yaptığı işlemler yer almaktadır
        private void btnTabloVeriOku_Click(object sender, EventArgs e)
        {
            text.Clear();//text dosyası temizleniyor.
            //Bu kod parçacığı tablo sekmesindeki döküman textboxına veriyi çeker 
            //ve aynı zamanda text değişkenine veriyi atar
            rtbTabloDokuman.Text = ReadPdfFile(veriOku());
        }
        private void btnTabloBul_Click(object sender, EventArgs e)
        {
            //Maksimum 15*15 tablo belirlenmiştir.
            rtbTabloRapor.Clear();//Tablo sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int tabloSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 15; i++)
            {
                for (int j = 1; j <= 15; j++)
                {
                    tabloSayisi = tabloBul(i,j, text.ToString());//tablo bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (tabloSayisi == 2)//tablo sayısı 0 dan farklı ise kaynak bulunmuştur.
                    {
                        rtbTabloRapor.Text += "Tablo " + i + "." + j +" "+tabloSayisi+" tane bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbTabloRapor.Text += "Tablo " + i + "." + j + "  bulunmamaktadır.\n";//tablonun bulunmadığı rapor edilir.
                    }
                }               
            }
        }
        private void btnTabloAtifBul_Click(object sender, EventArgs e)
        {
            rtbTabloRapor.Clear();//Tablo sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int tabloSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 15; i++)
            {
                for (int j = 1; j <= 15; j++)
                {
                    tabloSayisi = tabloAtifBul(i, j, text.ToString());//tablo atıf bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (tabloSayisi != 0)//tablo sayısı 0 dan farklı ise kaynak bulunmuştur.
                    {
                        rtbTabloRapor.Text += "Tablo " + i + "." + j + " " + tabloSayisi + " kere atıf yapılmıştır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbTabloRapor.Text += "Tablo " + i + "." + j + "  atıf bulunmamaktadır.\n";//Tablonun bulunmadığı rapor edilir.
                    }
                }

            }
        }
        private void btnTabloBlokAtifBul_Click(object sender, EventArgs e)
        {
            //Tablolarda blok atıf bulunmadığı için uyarı mesajını gösterdik.
            MessageBox.Show("Tablolarda blok atıf bulunmamaktadır.", "UYARI",MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
        #region Tablo Metot
        //Burada Tablo sekmesindeki yapılan işlemlerin metotları bulunmaktadır.
        public static int tabloBul(int tablono1,int tablono2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Tablo." + tablono1 + "\\." + tablono2 + "\\."; //Aranmak istenilen regex ifadesi tanımlandı. Tablo 5.2. ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int tabloAtifBul(int tablono1, int tablono2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Tablo." + tablono1 + "\\." + tablono2 + "\\’"; //Aranmak istenilen regex ifadesi tanımlandı. Tablo 5.2' ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        #endregion

        #region Şekil İşlemleri
        //Burada Şekil sekmesindeki butonların yaptığı işlemler yer almaktadır
        private void btnSekilVeriOku_Click(object sender, EventArgs e)
        {
            text.Clear();//text dosyası temizleniyor.
            //Bu kod parçacığı tablo sekmesindeki döküman textboxına veriyi çeker 
            //ve aynı zamanda text değişkenine veriyi atar
            rtbSekilDokuman.Text = ReadPdfFile(veriOku());
        }
        private void btnSekilBul_Click(object sender, EventArgs e)
        {
            rtbSekilRapor.Clear();//Şekil sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int sekilSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 20; i++)//Burada şekil limiti 20*40 olarak belirlenmiştir.
            {
                for (int j = 1; j <= 40; j++)
                {
                    sekilSayisi = sekilBul(i,j,text.ToString());//sekil bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (sekilSayisi >= 2)//sekil sayısı 2 ye eşit veya büyük ise sekil bulunmuştur.
                    {
                        rtbSekilRapor.Text += "Şekil " + i + "." + j + ". " + sekilSayisi + " tane bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbSekilRapor.Text += "Şekil " + i + "." + j + ". bulunmamaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                }
            }
        }
        private void btnSekilAtifBul_Click(object sender, EventArgs e)
        {
            rtbSekilRapor.Clear();//Şekil sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int sekilSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 20; i++)//Burada şekil limiti 20*40 olarak belirlenmiştir.
            {
                for (int j = 1; j <= 40; j++)
                {
                    sekilSayisi = sekilAtifBul(i, j, text.ToString());//sekil atif bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (sekilSayisi >= 1)//sekil sayısı 1 den fazla ise sekil atıf bulunmuştur.
                    {
                        rtbSekilRapor.Text += "Şekil " + i + "." + j + "’ " + sekilSayisi + " tane atıf bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbSekilRapor.Text += "Şekil " + i + "." + j + "’ bulunmamaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                }
            }
        }
        private void btnSekilBlokAtifBul_Click(object sender, EventArgs e)
        {
            rtbSekilRapor.Clear();//Şekil sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int sekilSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 20; i++)
            {
                for (int j = 1; j <= 40; j++)
                {
                    for (char k = 'a'; k <= 'j'; k++)//Karakterli sekillere ulaşılması için bu döngü tanımlanmıştır. Sınırımız a-j 'ye kadardır.
                    {
                        sekilSayisi = sekilBlokAtifBul(i, j, k, text.ToString());//sekil blok atif bul metodundan gelen eşleşme sayısı değişkene atandı.
                        if (sekilSayisi != 0)
                        {
                            rtbSekilRapor.Text += "Şekil " + i + "." + j + k + "  " + sekilSayisi + " tane blok atıf bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                        }      
                    }
                }
            }
        }
        #endregion
        #region Şekil Metot
        //Burada Şekil sekmesindeki yapılan işlemlerin metotları bulunmaktadır.
        public static int sekilBul(int sekilno1, int sekilno2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Şekil." + sekilno1 + "\\." + sekilno2 + "\\."; //Aranmak istenilen regex ifadesi tanımlandı. Şekil 5.2. ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int sekilAtifBul(int sekilno1, int sekilno2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Şekil." + sekilno1 + "\\." + sekilno2 + "\\’"; //Aranmak istenilen regex ifadesi tanımlandı. Şekil 5.2’ ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int sekilBlokAtifBul(int sekilno1, int sekilno2, char karakter, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Şekil." + sekilno1 + "\\." + sekilno2 + karakter; //Aranmak istenilen regex ifadesi tanımlandı. Şekil 5.2a ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        #endregion

        #region Denklem İşlemleri
        //Burada Diğer sekmesinde bulunan denklem işlemlerini yapan butonlar yer almaktadır
        private void btnDigerVeriOku_Click(object sender, EventArgs e)
        {
            text.Clear();//text dosyası temizleniyor.
            //Bu kod parçacığı kaynak sekmesindeki döküman textboxına veriyi çeker 
            //ve aynı zamanda text değişkenine veriyi atar
            rtbDigerDokuman.Text = ReadPdfFile(veriOku());
        }

        private void btnDigerDenklemBul_Click(object sender, EventArgs e)
        {
            //Maksimum 10*15 tablo belirlenmiştir.
            rtbDigerRapor.Clear();//Diğer sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int denklemSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 15; j++)
                {
                    denklemSayisi = denklembul(i, j, text.ToString());//denklem bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (denklemSayisi >= 1)//denklem sayısı 1 e eşit ve 1 den büyük ise denklem bulunmuştur.
                    {
                        rtbDigerRapor.Text += "Denklem " + i + "." + j + " " + denklemSayisi + " tane bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbDigerRapor.Text += "Denklem " + i + "." + j + "  bulunmamaktadır.\n";//Denklemin bulunmadığı rapor edilir.
                    }
                }
            }
        }

        private void btnDigerDenklemAtifBul_Click(object sender, EventArgs e)
        {
            //Maksimum 10*15 tablo belirlenmiştir.
            rtbDigerRapor.Clear();//Diğer sekmesindeki raporların yazıldığı richtextbox 'ı temizler
            int denklemSayisi = 0;//Bulunan değerlerin atamasının yapıldığı değişkendir.
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 15; j++)
                {
                    denklemSayisi = denklemAtifBul(i, j, text.ToString());//denklem atıf bul metodundan gelen eşleşme sayısı değişkene atandı.
                    if (denklemSayisi >= 1)//denklem sayısı 1 e eşit ve 1 den büyük ise denklem atıf bulunmuştur.
                    {
                        rtbDigerRapor.Text += "(" + i + "." + j + ") " + denklemSayisi + " tane bulunmaktadır.\n";//Raporlama gerçekleştirilir.
                    }
                    else
                    {
                        rtbDigerRapor.Text += "(" + i + "." + j + ")  bulunmamaktadır.\n";//denklemin atıf yapılmadığı rapor edilir.
                    }
                }
            }
        }
        #endregion
        #region Denklem Metot
        //Burada Diğer sekmesinde bulunan denklem metotları yer almaktadır
        public static int denklembul(int denklemNo1, int denklemNo2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"Denklem."+denklemNo1+"\\."+denklemNo2+ "\\’"; //Aranmak istenilen regex ifadesi tanımlandı. Denklem 2.1 ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        public static int denklemAtifBul(int denklemNo1, int denklemNo2, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"\(" + denklemNo1 + "\\." + denklemNo2 + "\\)"; //Aranmak istenilen regex ifadesi tanımlandı. (2.1) ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        #endregion

        #region Liste İşlemleri
        private void btnListeVeriOku_Click(object sender, EventArgs e)
        {
            text.Clear();//text dosyası temizleniyor.
            //Bu kod parçacığı kaynak sekmesindeki döküman textboxına veriyi çeker 
            //ve aynı zamanda text değişkenine veriyi atar
            rtbListeDokuman.Text = ReadPdfFile(veriOku());
        }
        private void btnListeTabloKontrol_Click(object sender, EventArgs e)
        {
            rtbListeRapor.Clear();
            if (ListeBul("TABLOLAR", text.ToString())  == 2)
            {
                rtbListeRapor.Text += "TABLOLAR LİSTESİ İÇİNDEKİLER KISMINDA VE\r BAŞLIK OLARAK BULUNMAKTADIR. \n";
            }
            else
            {
                rtbListeRapor.Text += "TABLOLAR LİSTESİ BULUNMAMAKTADIR. \n";
            }
            
        }
        private void btnListeSekilKontrol_Click(object sender, EventArgs e)
        {
            rtbListeRapor.Clear();
            if (ListeBul("ŞEKİLLER", text.ToString()) == 2)
            {
                rtbListeRapor.Text += "ŞEKİLLER LİSTESİ İÇİNDEKİLER KISMINDA VE\r BAŞLIK OLARAK BULUNMAKTADIR. \n";
            }
            else
            {
                rtbListeRapor.Text += "ŞEKİLLER LİSTESİ BULUNMAMAKTADIR. \n";
            }
        }
        private void btnListeEklerKontrol_Click(object sender, EventArgs e)
        {
            rtbListeRapor.Clear();
            if (ListeBul("EKLER", text.ToString()) == 2)
            {
                rtbListeRapor.Text += "EKLER LİSTESİ İÇİNDEKİLER KISMINDA VE\r BAŞLIK OLARAK BULUNMAKTADIR. \n";
            }
            else
            {
                rtbListeRapor.Text += "EKLER LİSTESİ BULUNMAMAKTADIR. \n";
            }
        }
        #endregion
        #region Liste Metot
        public static int ListeBul(string listeIsim, string kaynak_text)
        {
            string text = kaynak_text;//Parametre olarak alınan ana metin değişkene aktarıldı.
            string pat = @"" + listeIsim + ".LİSTESİ"; //Aranmak istenilen regex ifadesi tanımlandı. [1] ve benzeri

            // Düzenli ifade nesnesi tanımlandı
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match nesnesi oluşturduk ve içerisinde değer eşleşmesi başlatıldı.
            Match m = r.Match(text);
            int matchCount = 0;//Eşleşme sayısı değişkeni tanımlandı.
            while (m.Success)
            {
                ++matchCount;
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        Capture c = cc[j];
                    }
                }
                m = m.NextMatch();
            }
            return matchCount;//Bulunan değer gönderildi.
        }
        #endregion


    }
}

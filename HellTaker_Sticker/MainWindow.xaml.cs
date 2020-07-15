using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WMPLib;

/* # 스티커 애니메이션
 * 100 X 100 px ; 12 개 이미지 스티커 
 */

namespace HellTaker_Sticker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global Variables
        /* Stickers */
        System.Windows.Forms.MenuItem Lucifer, Judgement, Azazel, Cerberus, Justice, Malina, Modeus, Pandemonica, Zdrada;
        List<System.Windows.Forms.MenuItem> sticker_List = new List<System.Windows.Forms.MenuItem>();
        string[] sticker_Name = new string[] { "Lucifer", "Judgement", "Azazel", "Cerberus", "Justice", "Malina", "Modeus", "Pandemonica", "Zdrada" };
        int currentSticker = 0;

        /* Sprite */
        Bitmap original;
        Bitmap[] frams = new Bitmap[12];
        Bitmap icon = new Bitmap(100, 100);
        ImageSource[] imgFrame = new ImageSource[12];
        string bitmapPath = "Resources/Sprites/";
        string bitmapName = "Lucifer.png";      // base

        /* Animation */
        int frame = -1;
        string iconPath = "Resources/Sprites/Icon.png";

        /* menu */
        System.Windows.Forms.NotifyIcon noti;

        /* Music */
        WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        List<System.Windows.Forms.MenuItem> music_List = new List<System.Windows.Forms.MenuItem>();
        string[] music_Name = new string[] { "Vitality", "Luminescent", "Epitomize", "Apropos" };
        string music_playPath = "Resources/Musics/";
        string music_playName = "Vitality.mp3";     //base
        Boolean is_Play = true;
        int currentMusic = 0;
        #endregion

        /* for release bitmap */
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /* Singleton Pattern */
        private static readonly Lazy<MainWindow> instance =
            new Lazy<MainWindow>(() => new MainWindow());
        public static MainWindow Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            /* Form Setting */
            this.Topmost = true;
            /* Create Base Frame */
            CreateFrame();

            /* Import Icon Image */
            Bitmap icon_Image = System.Drawing.Image.FromFile(iconPath) as Bitmap;

            /* Flow Frame */
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.0167 * 2);  // 60FPS
            timer.Tick += NextFrame;
            timer.Start();

            /* Mouse Event */
            MouseDown += MainWindow_MouseDown;
            MouseDoubleClick += MainWindow_MouseDoubleClick;

            #region ContextMenu
            /* for notify icon */
            int notify_Index = 0;
            var menu = new System.Windows.Forms.ContextMenu();
            noti = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.FromHandle(icon_Image.GetHicon()),
                Visible = true,
                Text = "Sticker_Setting",
                ContextMenu = menu,
            };
            var stickers = new System.Windows.Forms.MenuItem
            {
                Index = notify_Index++,
                Text = "Stickers"
            };
            var musicPlayer = new System.Windows.Forms.MenuItem
            {
                Index = notify_Index++,
                Text = "MusicPlayer"
            };
            var exit = new System.Windows.Forms.MenuItem
            {
                Index = notify_Index++,
                Text = "Bye~!",
            };

            // Click Event
            exit.Click += (Object o, EventArgs e) =>
            {
                noti.Visible = false;
                System.Windows.Application.Current.Shutdown();
            };

            menu.MenuItems.Add(stickers);
            menu.MenuItems.Add(musicPlayer);
            menu.MenuItems.Add(exit);
            noti.ContextMenu = menu;
            #endregion

            #region Make Stickers MenuItem
            /* Create Sticker MenuItem */
            int stk_Index = 0;

            Lucifer = CreateStickerMenuItem(stk_Index++);
            Judgement = CreateStickerMenuItem(stk_Index++);
            Azazel = CreateStickerMenuItem(stk_Index++);
            Cerberus = CreateStickerMenuItem(stk_Index++);
            Justice = CreateStickerMenuItem(stk_Index++);
            Malina = CreateStickerMenuItem(stk_Index++);
            Modeus = CreateStickerMenuItem(stk_Index++);
            Pandemonica = CreateStickerMenuItem(stk_Index++);
            Zdrada = CreateStickerMenuItem(stk_Index++);

            /* Create Sticker_List */
            sticker_List.Add(Lucifer);
            sticker_List.Add(Judgement);
            sticker_List.Add(Azazel);
            sticker_List.Add(Cerberus);
            sticker_List.Add(Justice);
            sticker_List.Add(Malina);
            sticker_List.Add(Modeus);
            sticker_List.Add(Pandemonica);
            sticker_List.Add(Zdrada);

            /* Insert Sticker MenuItems */
            for (int i = 0; i < stk_Index; i++)
            {
                stickers.MenuItems.Add(sticker_List[i]);
            }

            #region Click Sticker MenuItem
            Lucifer.Click += (Object o, EventArgs e) => ClickSticker(Lucifer);
            Judgement.Click += (Object o, EventArgs e) => ClickSticker(Judgement);
            Azazel.Click += (Object o, EventArgs e) => ClickSticker(Azazel);
            Cerberus.Click += (Object o, EventArgs e) => ClickSticker(Cerberus);
            Justice.Click += (Object o, EventArgs e) => ClickSticker(Justice);
            Malina.Click += (Object o, EventArgs e) => ClickSticker(Malina);
            Modeus.Click += (Object o, EventArgs e) => ClickSticker(Modeus);
            Pandemonica.Click += (Object o, EventArgs e) => ClickSticker(Pandemonica);
            Zdrada.Click += (Object o, EventArgs e) => ClickSticker(Zdrada);
            #endregion
            #endregion

            #region Make MusicPlayer
            /*Create Item*/
            var Music_Play = new System.Windows.Forms.MenuItem
            {
                Index = 0,
                Text = "Play/Stop",
            };
            var Music_PlayList = new System.Windows.Forms.MenuItem
            {
                Index = 1,
                Text = "MusicList",
            };

            /* Insert Item */
            musicPlayer.MenuItems.Add(Music_Play);
            musicPlayer.MenuItems.Add(Music_PlayList);

            /*Click Item*/
            Music_Play.Click += (Object o, EventArgs e) =>
            {
                if (is_Play)
                {
                    Music_Play.Checked = false;
                    wplayer.controls.stop();
                }
                else
                {
                    Music_Play.Checked = true;
                    wplayer.controls.play();
                }

                is_Play = !is_Play;
            };

            #region MusicList
            /* Create Item */
            int mpList_Index = 0;

            var Vitality = CreateMusicrMenuItem(mpList_Index++);
            var Apropos = CreateMusicrMenuItem(mpList_Index++);
            var Epitomize = CreateMusicrMenuItem(mpList_Index++);
            var Luminescent = CreateMusicrMenuItem(mpList_Index++);

            /* Create List */
            music_List.Add(Vitality);
            music_List.Add(Apropos);
            music_List.Add(Epitomize);
            music_List.Add(Luminescent);

            /* Insert Item */
            for (int i = 0 ; i < mpList_Index; i++)
            {
                Music_PlayList.MenuItems.Add(music_List[i]);
            }

            #region Create Click
            /* Click Item */
            Vitality.Click += (Object o, EventArgs e) => ClickMusic(Vitality);
            Apropos.Click += (Object o, EventArgs e) => ClickMusic(Apropos);
            Epitomize.Click += (Object o, EventArgs e) => ClickMusic(Epitomize);
            Luminescent.Click += (Object o, EventArgs e) => ClickMusic(Luminescent);
            #endregion
            #endregion
            #endregion

            /* Audio Start */
            wplayer.settings.setMode("loop", true);
            wplayer.URL = music_playPath + music_playName;
            wplayer.controls.play();

            Music_Play.Checked = true;
            music_List[currentMusic].Checked = true;
        }

        #region new System.Windows.Forms.MenuItem & Click Listener Function
        private System.Windows.Forms.MenuItem CreateStickerMenuItem(int sticker_Index)
        {
            var item = new System.Windows.Forms.MenuItem()
            {
                Index = sticker_Index,
                Text = sticker_Name[sticker_Index]
            };

            return item;
        }

        private void ClickSticker(System.Windows.Forms.MenuItem _sticker)
        {
            //Debug.WriteLine(_sticker.Text);
            bitmapName = _sticker.Text + ".png";
            CreateFrame();

            sticker_List[currentSticker].Checked = false;
            currentSticker = _sticker.Index;
            sticker_List[currentSticker].Checked = true;
        }

        private System.Windows.Forms.MenuItem CreateMusicrMenuItem(int mpList_Index)
        {
            var item = new System.Windows.Forms.MenuItem()
            {
                Index = mpList_Index,
                Text = music_Name[mpList_Index]
            };

            return item;
        }

        private void ClickMusic(System.Windows.Forms.MenuItem _music)
        {
            music_playName = _music.Text + ".mp3";
            wplayer.URL = music_playPath + music_playName;
            Debug.WriteLine("{0}", wplayer.URL);

            music_List[currentMusic].Checked = false;
            currentMusic = _music.Index;
            music_List[currentMusic].Checked = true;
        }
        #endregion

        #region Animation Func
        /* Cut Multi Sprite to Single Sprites */
        private void CreateFrame()
        {
            original = System.Drawing.Image.FromFile(bitmapPath+bitmapName) as Bitmap;

            for (int i = 0; i < 12; i++)
            {
                frams[i] = new Bitmap(100, 100);
                using (Graphics g = Graphics.FromImage(frams[i]))
                {
                    g.DrawImage(original,
                        new System.Drawing.Rectangle(0, 0, 100, 100),
                        new System.Drawing.Rectangle(i * 100, 0, 100, 100),
                        GraphicsUnit.Pixel);
                }

                var handle = frams[i].GetHbitmap();

                try
                {
                    imgFrame[i] = Imaging.CreateBitmapSourceFromHBitmap(handle,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
        }

        private void NextFrame(object sender, EventArgs e)
        {
            frame = (frame + 1) % 12;
            iSticker.Source = imgFrame[frame];
        }
        #endregion

        #region Mouse Action Func
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            noti.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }
        #endregion
    }
}

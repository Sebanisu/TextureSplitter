using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextureSplitter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region Fields

        private int Cols;
        private GraphicsDeviceManager graphics;
        private int Rows;
        private SpriteBatch spriteBatch;
        private GameTime _gametime;

        #endregion Fields

        #region Methods

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (SaveQ.Count >= TexQ.Count && SaveQ.Count>0)
            {
                (string path, string clean, Texture2D tex) = SaveQ.Dequeue();
                Save(path, clean, tex);
            }
            if (SaveQ.Count < TexQ.Count && TexQ.Count > 0)
            {
                // TODO: Add your drawing code here
                Tex = TexQ.Dequeue();
                if (Tex != null)
                {
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    spriteBatch.Begin();
                    float s = (float)graphics.PreferredBackBufferWidth / Tex.Width;
                    float s2 = (float)graphics.PreferredBackBufferHeight / Tex.Height;
                    spriteBatch.Draw(Tex, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(s <= s2 ? s : s2), SpriteEffects.None, 0);
                    spriteBatch.End();
                    Tex.Dispose();
                    Tex = null;
                }
                base.Draw(gameTime);
                //Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run. This is
        /// where it can query for any required services and load any non-graphic related content.
        /// Calling base.Initialize will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            FileQ = new Queue<(Action<string> Split, string Path)>();
            TexQ = new Queue<Texture2D>();
            SaveQ = new Queue<(string path, string clean, Texture2D tex)>();
            Rmatch = new Regex(@"_\d+x\d+", RegexOptions.IgnoreCase);
            foreach (string a in Program.Args)
            {
                string safe = a.Trim('"');
                if (File.Exists(safe) && safe.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    FileQ.Enqueue((Split, safe));
                else if (Directory.Exists(safe))
                {
                    IEnumerable<string> list = Directory.GetFiles(safe, "*", SearchOption.AllDirectories)
                        .Where(x => x.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                    foreach (string f in list)
                        FileQ.Enqueue((Split, f));
                }
            }
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 512;
            graphics.PreferredBackBufferWidth = 512;
            graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent() =>
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        // TODO: use this.Content to load your game content here
        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions,
        /// gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _gametime = gameTime;
            if (TexQ.Count == 0 && FileQ.Count == 0 && SaveQ.Count == 0)
                Exit();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (TexQ.Count == 0 && FileQ.Count > 0)
            {
                (Action<string> Split, string Path) f = FileQ.Dequeue();
                f.Split(f.Path);
                SuppressDraw();
            }

            base.Update(gameTime);
        }

        #endregion Methods

        #region Constructors

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #endregion Constructors

        #region Properties

        public Queue<(Action<string> Split, string Path)> FileQ { get; private set; }
        public Regex Rmatch { get; private set; }
        public Texture2D Tex { get; private set; }
        public Queue<Texture2D> TexQ { get; private set; }
        public Queue<(string path, string clean, Texture2D tex)> SaveQ { get; private set; }

        #endregion Properties

        public Texture2D LoadPNG(string path, int palette = -1)
        {
            Texture2D tex = null;
            using (FileStream fs = File.OpenRead(path))
            {
                tex = Texture2D.FromStream(graphics.GraphicsDevice, fs);
            }
            return tex;
        }

        public void Save(string path, string clean, Texture2D tex)
        {
            //string clean = Path.GetFileNameWithoutExtension(Regex.Replace(Filename, @"{[^}]+}", ""));
            string outpath = Path.Combine(path, $"{clean}.png");
            Console.WriteLine($"Saving {tex.Width}x{tex.Height} texture => {outpath}");
            using (FileStream fs = File.Create(outpath))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }

        public void Split(string safe)
        {
            if (Rmatch.IsMatch(Path.GetFileName(safe)))
                return;
            Texture2D texin = LoadPNG(safe);
            Rows = Cols = 1;
            if (texin.Width == texin.Height)
                Rows = Cols = 2;
            else if (texin.Width / 2 == texin.Height)
                Rows = 2;
            if (Rows * Cols > 1)
            {
                Console.WriteLine($"Splitting {texin.Width}x{texin.Height} texture => {Rows * Cols} files");
                TexQ.Enqueue(texin);
                int RowHeight = texin.Height / Rows;
                int ColWidth = texin.Width / Cols;
                Rectangle src = new Rectangle();
                for (int r = 0; r < Rows; r++)
                {
                    src.Y += r > 0 ? RowHeight : 0;
                    for (int c = 0; c < Cols; c++)
                    {
                        Texture2D tex = new Texture2D(graphics.GraphicsDevice, ColWidth, RowHeight, false, SurfaceFormat.Color);
                        src.Height = RowHeight;
                        src.Width = ColWidth;
                        src.X += c > 0 ? ColWidth : 0;
                        Color[] buffer = new Color[src.Height * src.Width];
                        texin.GetData(0, src, buffer, 0, buffer.Length);
                        tex.SetData(buffer);
                        TexQ.Enqueue(tex);
                        string clean = $"{Path.GetFileNameWithoutExtension(safe)}_{c}x{r}";
                        string path = Path.GetDirectoryName(safe);
                        SaveQ.Enqueue((path, clean, tex));
                    }
                    src.X = 0;
                }
            }
        }
    }
}
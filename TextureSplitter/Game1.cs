using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private int Cols;
        private int Rows;

        public GameTime GameTime { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run. This is
        /// where it can query for any required services and load any non-graphic related content.
        /// Calling base.Initialize will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize() =>
            // TODO: Add your initialization logic here

            base.Initialize();

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent() =>
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);// TODO: use this.Content to load your game content here

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions,
        /// gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

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
            Texture2D texin = LoadPNG(safe);
            Rows = Cols = 1;
            if (texin.Width == texin.Height)
                Rows = Cols = 2;
            else if (texin.Width / 2 == texin.Height)
                Rows = 2;
            if (Rows * Cols > 1)
            {
                Console.WriteLine($"Splitting {texin.Width}x{texin.Height} texture => {Rows*Cols} files");
                int RowHeight = texin.Height / Rows;
                int ColWidth = texin.Width / Cols;
                //spriteBatch.Begin();
                //spriteBatch.Draw(texin, new Rectangle(0, 0, texin.Width, texin.Height),Color.White);
                //spriteBatch.End();
                //base.Draw(GameTime);
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
                        //spriteBatch.Begin();
                        //spriteBatch.Draw(tex, new Rectangle(0, 0, tex.Width, tex.Height), Color.White);
                        //spriteBatch.End();
                        base.Draw(GameTime);
                        string clean = $"{Path.GetFileNameWithoutExtension(safe)}_{c}x{r}";
                        string path = Path.GetDirectoryName(safe);
                        Save(path, clean, tex);

                    }
                    src.X = 0;
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GameTime = gameTime;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            foreach (string a in Program.Args)
            {
                string safe = a.Trim('"');
                if (File.Exists(safe) && safe.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    Split(safe);
                }
                else if (Directory.Exists(safe))
                {
                    System.Collections.Generic.IEnumerable<string> list = Directory.GetFiles(safe, "*", SearchOption.AllDirectories)
                        .Where(x => x.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                    foreach (string f in list)
                    {
                        Split(f);
                    }
                }
            }
            // TODO: Add your drawing code here
            Exit();
        }
    }
}
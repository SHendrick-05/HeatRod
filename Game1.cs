using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace HeatRod
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D rectangle;
        private SpriteFont sFont;
        int frame = 0;
        bool count = true;

        private Rectangle xAxis;
        private Rectangle yAxis;

        private double[,] heats;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }



        protected override void Initialize()
        {
            // Set Fullscreen
            if (GraphicsDevice == null)
                _graphics.ApplyChanges();
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

            // Init texture
            rectangle = new Texture2D(GraphicsDevice, 1, 1);
            rectangle.SetData(new Color[] { Color.White });

            // Initialise axes
            yAxis = new Rectangle(100, 100, 1, _graphics.PreferredBackBufferHeight - 200);
            xAxis = new Rectangle(100, _graphics.PreferredBackBufferHeight / 2, _graphics.PreferredBackBufferWidth - 200, 1);

            // Initialise plotting variables.
            heats = new double[xAxis.Width, 60*5];

            sFont = Content.Load<SpriteFont>("Font");

            if (!File.Exists("plot.json"))
            {

                // Plot the function.
                double range = Calculation.upperX - Calculation.lowerX;
                for (int i = 0; i < xAxis.Width; i++)
                {
                    for (int j = 0; j < 60 * 5; j++)
                    {
                        double proportion = (double)i / xAxis.Width;
                        double X = Calculation.lowerX + (proportion * range);
                        double Y = Calculation.fourierTime.Evaluate(new CenterSpace.NMath.Core.DoubleVector(X, j / 60d));
                        heats[i, j] = Y;
                    }
                }
                string text = JsonConvert.SerializeObject(heats);
                File.WriteAllText("plot.json", text);
                Exit();
            }
            else
            {
                string text = File.ReadAllText("plot.json");
                heats = JsonConvert.DeserializeObject<double[,]>(text);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            count = !count;
            if (frame < 299 && count)
                frame++;
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the axes
            _spriteBatch.Begin();
            _spriteBatch.Draw(rectangle, xAxis, Color.White);
            _spriteBatch.Draw(rectangle, yAxis, Color.White);

            double[] plots = heats.SliceRow(frame).ToArray();
            double max = plots.Max();
            double min = plots.Min();

            // Draw the points
            double scalar = 1;
            if (max > Calculation.upperY && min < Calculation.lowerY)
            {
                scalar = Math.Min(Calculation.upperY / max, Calculation.lowerY / min);
            }
            else if (max > Calculation.upperY)
            {
                scalar = Calculation.upperY / max;
            }
            else if (min < Calculation.lowerY)
            {
                scalar = Calculation.lowerY / min;
            }

            Vector2 prevPos = Vector2.Zero;
            for(int i = 0; i < plots.Length; i++) 
            {
                double Y = 100 + (yAxis.Height / 2) - (plots[i] * scalar * (yAxis.Height / 2));
                Vector2 position = new Vector2(100 + i, (float)Y);
                if (i != 0)
                {
                    DrawLine(prevPos, position);
                }
                prevPos = position;
            }

            //_spriteBatch.DrawString(sFont, Calculation.fourierTime.Evaluate(1).ToString(), new Vector2(0, 0), Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        internal void DrawLine(Vector2 start, Vector2 end)
        {
            _spriteBatch.Draw(
                rectangle,
                start,
                null,
                Color.Black,
                (float)Math.Atan2(end.Y - start.Y, end.X - start.X),
                new Vector2(0f, (float)rectangle.Height / 2),
                new Vector2(Vector2.Distance(start, end), 1f),
                SpriteEffects.None,
                0f);
        }
    }
}
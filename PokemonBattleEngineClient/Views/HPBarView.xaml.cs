﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Kermalis.PokemonBattleEngine.Battle;
using Kermalis.PokemonBattleEngine.Data;
using Kermalis.PokemonBattleEngineClient.Infrastructure;
using System;
using System.ComponentModel;

namespace Kermalis.PokemonBattleEngineClient.Views
{
    public sealed class HPBarView : UserControl, INotifyPropertyChanged
    {
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
        public new event PropertyChangedEventHandler PropertyChanged;

        private Point _location;
        public Point Location
        {
            get => _location;
            internal set
            {
                if (!_location.Equals(value))
                {
                    _location = value;
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        private static SolidColorBrush _greenSides, _greenMid, _yellowSides, _yellowMid, _redSides, _redMid;

        public HPBarView()
        {
            if (_greenSides == null)
            {
                _greenSides = new SolidColorBrush(0xFF008C29);
                _greenMid = new SolidColorBrush(0xFF00FF4A);
                _yellowSides = new SolidColorBrush(0xFF9C6310);
                _yellowMid = new SolidColorBrush(0xFFF7B500);
                _redSides = new SolidColorBrush(0xFF942131);
                _redMid = new SolidColorBrush(0xFFFF3142);
            }

            DataContext = this;
            AvaloniaXamlLoader.Load(this);
        }

        internal void Update(PBEPokemon pkmn, bool showRawValues)
        {
            var wb = new WriteableBitmap(new PixelSize(104, 27), new Vector(96, 96), PixelFormat.Bgra8888);
            using (IRenderTarget rtb = Utils.RenderInterface.CreateRenderTarget(new[] { new WriteableBitmapSurface(wb) }))
            using (IDrawingContextImpl ctx = rtb.CreateDrawingContext(null))
            {
                string barResource;
                byte yOffset;
                if (showRawValues)
                {
                    barResource = "MISC.HPBAR_Ally.png";
                    yOffset = 0;
                }
                else
                {
                    barResource = "MISC.HPBAR_Foe.png";
                    yOffset = 2;
                }
                var hpBar = new Bitmap(Utils.GetResourceStream(barResource));
                ctx.DrawImage(hpBar.PlatformImpl, 1.0, new Rect(0, 0, hpBar.PixelSize.Width, hpBar.PixelSize.Height), new Rect(0, 11 + yOffset, hpBar.PixelSize.Width, hpBar.PixelSize.Height));

                Bitmap nickname = StringRenderer.Render(pkmn.KnownNickname, "BattleName");
                ctx.DrawImage(nickname.PlatformImpl, 1.0, new Rect(0, 0, nickname.PixelSize.Width, nickname.PixelSize.Height), new Rect(72 - Math.Max(54, nickname.PixelSize.Width), yOffset, nickname.PixelSize.Width, nickname.PixelSize.Height));

                PBEGender gender = pkmn.Status2.HasFlag(PBEStatus2.Transformed) ? pkmn.Gender : pkmn.KnownGender;
                Bitmap level = StringRenderer.Render($"{(gender == PBEGender.Female ? "♀" : gender == PBEGender.Male ? "♂" : " ")}[LV]{pkmn.Level}", "BattleLevel");
                ctx.DrawImage(level.PlatformImpl, 1.0, new Rect(0, 0, level.PixelSize.Width, level.PixelSize.Height), new Rect(70, 1 + yOffset, level.PixelSize.Width, level.PixelSize.Height));

                if (pkmn.Status1 != PBEStatus1.None)
                {
                    var status = new Bitmap(Utils.GetResourceStream("MISC.STATUS1_" + pkmn.Status1 + ".png"));
                    ctx.DrawImage(status.PlatformImpl, 1.0, new Rect(0, 0, status.PixelSize.Width, status.PixelSize.Height), new Rect(1, 11 + yOffset, status.PixelSize.Width, status.PixelSize.Height));
                }

                IBrush hpSides, hpMid;
                if (pkmn.HPPercentage <= 0.20)
                {
                    hpSides = _redSides;
                    hpMid = _redMid;
                }
                else if (pkmn.HPPercentage <= 0.50)
                {
                    hpSides = _yellowSides;
                    hpMid = _yellowMid;
                }
                else
                {
                    hpSides = _greenSides;
                    hpMid = _greenMid;
                }
                const byte lineW = 48;
                int theW = (int)(lineW * pkmn.HPPercentage);
                if (theW == 0 && pkmn.HPPercentage > 0)
                {
                    theW = 1;
                }
                ctx.FillRectangle(hpSides, new Rect(38, 13 + yOffset, theW, 1));
                ctx.FillRectangle(hpMid, new Rect(38, 13 + yOffset + 1, theW, 1));
                ctx.FillRectangle(hpSides, new Rect(38, 13 + yOffset + 2, theW, 1));

                if (showRawValues)
                {
                    Bitmap hp = StringRenderer.Render(pkmn.HP.ToString(), "BattleHP");
                    ctx.DrawImage(hp.PlatformImpl, 1.0, new Rect(0, 0, hp.PixelSize.Width, hp.PixelSize.Height), new Rect(62 - hp.PixelSize.Width, 16 + yOffset, hp.PixelSize.Width, hp.PixelSize.Height));
                    Bitmap maxHP = StringRenderer.Render(pkmn.MaxHP.ToString(), "BattleHP");
                    ctx.DrawImage(maxHP.PlatformImpl, 1.0, new Rect(0, 0, maxHP.PixelSize.Width, maxHP.PixelSize.Height), new Rect(70, 16 + yOffset, maxHP.PixelSize.Width, maxHP.PixelSize.Height));
                }
            }
            this.FindControl<Image>("Drawn").Source = wb;

            IsVisible = true;
        }
    }
}

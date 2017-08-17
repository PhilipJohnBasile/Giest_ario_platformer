﻿using MapEditor.Abstract;
using MapEditor.Enums;
using MapEditor.Handlers;
using MapEditor.Helpers;
using MapEditor.Interfaces;
using MapEditor.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MapEditor.Manager
{
    class MapManager : IGameObject
    {

        public static MapManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MapManager();
                return instance;
            }
        }
        
        public Boolean IsDebug
        {
            get
            {
                return debugMode;
            }
        }

        public SpriteFont DebugFont
        {
            get
            {
                return debugFont;
            }
        }

        public Vector2 Scale
        {
            get
            {
                return scale;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        private Vector2 scale;

        private SpriteFont debugFont;

        private bool debugMode;
        private bool isActive;

        private static MapManager instance;
        public ContentManager Content;
        public GraphicsDevice Graphics;
        public Viewport Viewport;
        public Camera Cam;
        private ObjectSourceManager objectSourceManager;
        private CollisionTypeManager typeManager;
        private Map map;

        private AMapButton saveFile;
        private AMapButton loadFile;
        private AMapButton clearMap;
        private AMapButton playerStart;
        private AMapButton mapTransitionStart;
        private AMapButton setMusicFile;

        private AMapButton setDrawType;
        private DrawType drawTypeSelected;

        private MapButtonType buttonSelected;



        public MapManager()
        {
        }

        public void Init()
        {
            scale = new Vector2(1,1);
            typeManager = new CollisionTypeManager();
            objectSourceManager = new ObjectSourceManager();
            objectSourceManager.Init();
            typeManager.Init();
            map = new Map();
            map.Init();
            debugMode = false;
            buttonSelected = MapButtonType.None;
            drawTypeSelected = DrawType.Both;

            clearMap = new AMapButton(new Vector2(900, 880), "Clear Map");
            saveFile = new AMapButton(new Vector2(1050, 920), "Save Map");
            loadFile = new AMapButton(new Vector2(1050, 880), "Load Map");
            playerStart = new AMapButton(new Vector2(900,920),"Set Start Pos");
            setMusicFile = new AMapButton(new Vector2(750, 920), "Set Music");
            mapTransitionStart = new AMapButton(new Vector2(750, 880), "Map Transition");
            setDrawType = new AMapButton(new Vector2(25, 800), "Draw: Both");
        }

        public void SetScale(Vector2 _scale)
        {
            this.scale = _scale;
        }
        

        public void Load()
        {
            debugFont = Content.Load<SpriteFont>("debugFont");
            map.Load();
            objectSourceManager.Load();
            clearMap.Load();
            saveFile.Load();
            loadFile.Load();
            setDrawType.Load();
            typeManager.Load();
            playerStart.Load();
            setMusicFile.Load();
            mapTransitionStart.Load();
        }

        public void Update(GameTime _gameTime)
        {
            //enabling disabling debugmode
            if (KeyboardManager.Instance.IsKeyActivity(Keys.O.ToString(), KeyActivity.Pressed))
            {
                debugMode = !debugMode;
            }

            if (MouseManager.Instance.IsKeyActivity(true, KeyActivity.Hold))
            {
                switch (buttonSelected)
                {
                    case MapButtonType.None:
                        map.SetTile(objectSourceManager.Cursor.Selected);
                    break;
                    case MapButtonType.PlayerPosition:
                        map.SetPlayerPosition(MouseManager.Instance.Position.ToVector2());
                    break;
                    
                }
                    
            }

            if (MouseManager.Instance.IsKeyActivity(false, KeyActivity.Hold))
            {
                map.ClearTile();
            }

            if (MouseManager.Instance.IsKeyActivity(true, KeyActivity.Pressed))
            {
                checkMouseFunctionality();
            }

            map.Update(_gameTime);
            
            objectSourceManager.Update(_gameTime);
            typeManager.Update(_gameTime);
        }

        public void SetActive(bool _isActive)
        {
            this.isActive = _isActive;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            
            objectSourceManager.Draw(_spriteBatch);
            map.Draw(_spriteBatch);
            saveFile.Draw(_spriteBatch);
            loadFile.Draw(_spriteBatch);
            clearMap.Draw(_spriteBatch);
            setDrawType.Draw(_spriteBatch);
            typeManager.Draw(_spriteBatch);
            setMusicFile.Draw(_spriteBatch);
            playerStart.Draw(_spriteBatch, buttonSelected == MapButtonType.PlayerPosition);
            mapTransitionStart.Draw(_spriteBatch, buttonSelected == MapButtonType.MapTransition);


            Texture2D texture = objectSourceManager.getTexture();
            Texture2D texture2 = typeManager.getTexture();
            Rectangle selectedSource = objectSourceManager.Cursor.Selected.Source;
            Rectangle collisionSection = new Rectangle(67, 497, 32, 32);
            Rectangle tileBox = new Rectangle(34, 497, 32, 32);




            _spriteBatch.DrawString(debugFont, "Selected", new Vector2(35, 380), Color.Pink);
            if (drawTypeSelected != DrawType.Collision)
            {
              
                _spriteBatch.Draw(texture, new Rectangle(34, 400, 64, 64), selectedSource, Color.White);
                SpriteBatchAssist.DrawBox(_spriteBatch, texture2, tileBox);

            }
            if (drawTypeSelected != DrawType.Tile)
            {
              
                _spriteBatch.Draw(texture2, new Rectangle(34, 400, 64, 64), Constant.GetCollisionColor(typeManager.Cursor.Selected.Type));
                SpriteBatchAssist.DrawBox(_spriteBatch, texture2, collisionSection);

            }


            
            _spriteBatch.Draw(texture, new Vector2(34, 497), selectedSource, Color.White * (drawTypeSelected != DrawType.Collision? 1f: .33f));
            
            _spriteBatch.Draw(texture2, collisionSection, Constant.GetCollisionColor(typeManager.Cursor.Selected.Type)*(drawTypeSelected != DrawType.Tile ? 1f : .33f));

        }

        public void SetGraphicsDevice(GraphicsDevice _graphicsDevice)
        {
            this.Graphics = _graphicsDevice;
            
        }

        public void SetViewport(Viewport _viewport)
        {
            this.Viewport = _viewport;
            Cam = new Camera(_viewport);
        }

        public void SetContentManager(ContentManager _content)
        {
            this.Content = _content;
        }


        public Texture2D CreateColorTexture(int r, int g, int b, int a)
        {
            Texture2D rect = new Texture2D(this.Graphics, 1, 1);

            Color[] data = new Color[1];
            for (int i = 0; i < data.Length; ++i) data[i] = new Color(r, g, b, a);
            rect.SetData(data);

            return rect;

        }

        private void SetButton(MapButtonType _newType)
        {
            buttonSelected = buttonSelected == _newType ? MapButtonType.None : _newType;
        }

        private void checkMouseFunctionality()
        {
            if (saveFile.Intersects(MouseManager.Instance.Position))
            {
                map.SaveMap();
            }

            if (loadFile.Intersects(MouseManager.Instance.Position))
            {
                map.LoadMap();
            }

            if (clearMap.Intersects(MouseManager.Instance.Position))
            {
                map.Reset();
            }

            if (setMusicFile.Intersects(MouseManager.Instance.Position))
            {
                map.SetMusic();
            }

            if (setDrawType.Intersects(MouseManager.Instance.Position))
            {
                switch(drawTypeSelected){
                    case DrawType.Both:
                        drawTypeSelected = DrawType.Tile;
                        break;
                    case DrawType.Collision:
                        drawTypeSelected = DrawType.Both;
                        break;
                    case DrawType.Tile:
                        drawTypeSelected = DrawType.Collision;
                        break;
                    
                }
                setDrawType.setText("Draw:" + drawTypeSelected.ToString());
            }



            if (playerStart.Intersects(MouseManager.Instance.Position)){
                SetButton(MapButtonType.PlayerPosition);
            }

            if (mapTransitionStart.Intersects(MouseManager.Instance.Position))
            {
                SetButton(MapButtonType.MapTransition);
            }

            switch (buttonSelected)
            {
                case MapButtonType.MapTransition:
                    map.SetTransition(MouseManager.Instance.Position.ToVector2());
                break;
            }
        }
    }
}

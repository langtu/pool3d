using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Scenarios;
using XNA_PoolGame.Graphics.Models;

namespace XNA_PoolGame.Scene
{
    public enum SceneManagerEnum
    {
        StraightForward,
        Octree,
        BSP
    }
    public abstract class SceneManager
    {
        public Dictionary<CustomModelPart, bool> Drawn;
        protected Scenario scenario;

        public int totalItemDrawn;
        public int totalItems;
        public Collider collider;

        public bool debug;
        protected SceneManager(Scenario scenario)
        {
            this.scenario = scenario;
            Drawn = new Dictionary<CustomModelPart, bool>();
            totalItemDrawn = 0;
            totalItems = 0;
        }

        public abstract void BuildScene();
        public abstract void DrawScene(GameTime gameTime);
    }
}

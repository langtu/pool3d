using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Graphics;

namespace XNA_PoolGame.PoolTables
{
    public class Classic : PoolTable
    {
        public Classic(Game _game, String _modelName)
            : base(_game, _modelName)
        {
            CommonConstructor();
        }
        public Classic(Game _game)
            : base(_game, "Models\\poolTable2")
        {
            CommonConstructor();
        }
        
        private void CommonConstructor()
        {
            MAX_X = 309.832f;
            MIN_X = -309.832f;

            MAX_Z = 153.428f;
            MIN_Z = -153.428f;

            SURFACE_POSITION_Y = 164.024f; // 192.69f;
            FRICTION_SURFACE = 0.99f;
            BORDER_FRITCION = 0.95f;

            pocket_radius = World.ballRadius * 1.4f;
            //surface1 = new Vector3(MAX_X, SURFACEPOS_Y, MIN_Z);
            //surface2 = new Vector3(MIN_X, SURFACEPOS_Y, MAX_X);

            MIN_HEAD_X = MAX_X / 2;
            MIN_HEAD_Z = MIN_Z + World.ballRadius;

            MAX_HEAD_X = MAX_X - World.ballRadius;
            MAX_HEAD_Z = MAX_Z - World.ballRadius;

            FRICTION_SURFACE = 0.99f;
        }

        public override void BuildPockets()
        {
            pockets_bounds = new BoundingSphere[6];
            const float a = 1.4f;

            


            pockets_bounds[0] = new BoundingSphere(new Vector3(MIN_X - 6.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 6.5f), World.ballRadius * a);

            pockets_bounds[1] = new BoundingSphere(new Vector3(0, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 15.3f), World.ballRadius * a);

            pockets_bounds[2] = new BoundingSphere(new Vector3(MAX_X + 6.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 6.5f), World.ballRadius * a);

            pockets_bounds[3] = new BoundingSphere(new Vector3(MIN_X - 6.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 6.5f), World.ballRadius * a);

            pockets_bounds[4] = new BoundingSphere(new Vector3(0, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 15.3f), World.ballRadius * a);

            pockets_bounds[5] = new BoundingSphere(new Vector3(MAX_X + 6.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 6.5f), World.ballRadius * a);

            Console.WriteLine("virtual void de classic table...");
            base.BuildPockets();
        }

        public override void Initialize()
        {

            //////// 4 //////////// 5 ///////////
            //   * ---------- * ---------- *   //
            //   |     |                   |   //
            //   |     |                   |   //
            // 3 |     |               *   | 0 //
            //   |     |                   |   //
            //   |     |                   |   //
            //   * ---------- * ---------- *   //
            //////// 2 //////////// 1 ///////////

            //rails[0] = new BoundingBox(new Vector3(MIN_X, SURFACEPOS_Y, MIN_Z + 18.5f),
            //    new Vector3(MIN_X, SURFACEPOS_Y + World.ballRadius, MAX_Z - 21.0f));

            /////////////////////////////////////////////////////////////////////////////////////
            railsNormals = new Vector3[6];

            railsNormals[0] = Vector3.Right;

            railsNormals[1] = Vector3.Backward;
            railsNormals[2] = Vector3.Backward;

            railsNormals[3] = Vector3.Left;

            railsNormals[4] = Vector3.Forward;
            railsNormals[5] = Vector3.Forward;

            /////////////////////////////////////////////////////////////////////////////////////
            rails = new BoundingBox[6];

            float railoffset = pocket_radius + 12.0f;
            rails[0] = new BoundingBox(new Vector3(MIN_X, SURFACE_POSITION_Y, MIN_Z + railoffset),
                new Vector3(MIN_X, SURFACE_POSITION_Y + World.ballRadius, MAX_Z - railoffset));

            rails[1] = new BoundingBox(new Vector3(MIN_X + 24.5f, SURFACE_POSITION_Y, MIN_Z),
                new Vector3(-24.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z));

            rails[2] = new BoundingBox(new Vector3(24.5f, SURFACE_POSITION_Y, MIN_Z),
                new Vector3(MAX_X - 24.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z));

            rails[3] = new BoundingBox(new Vector3(MAX_X, SURFACE_POSITION_Y, MIN_Z + 24.5f),
                new Vector3(MAX_X, SURFACE_POSITION_Y + World.ballRadius, MAX_Z - 24.5f));

            rails[4] = new BoundingBox(new Vector3(24.5f, SURFACE_POSITION_Y, MAX_Z),
                new Vector3(MAX_X - 24.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z)); ;

            rails[5] = new BoundingBox(new Vector3(MIN_X + 24.5f, SURFACE_POSITION_Y, MAX_Z),
                new Vector3(-24.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z));

            inside_normals = new Vector3[12];
            insidebands_pockets = new OrientedBoundingBox[12];


            BuildPockets();

            //inside_normals[0] = new Vector3((float)Math.Cos(MathHelper.ToRadians(48)), 0, (float)-Math.Sin(MathHelper.ToRadians(48)));
            inside_normals[0]= Vector3.Zero;
            
            inside_normals[1] = new Vector3((float)-Math.Cos(MathHelper.ToRadians(48)), 0, (float)Math.Sin(MathHelper.ToRadians(48)));

            inside_normals[2] = new Vector3(1, 0, 0);
            inside_normals[3] = new Vector3(-1, 0, 0);

            inside_normals[4] = new Vector3((float)-Math.Cos(MathHelper.ToRadians(48 + 90)), 0, (float)Math.Sin(MathHelper.ToRadians(48 + 90)));
            inside_normals[5] = new Vector3((float)Math.Cos(MathHelper.ToRadians(41 + 90)), 0, (float)-Math.Sin(MathHelper.ToRadians(41 + 90)));

            inside_normals[6] = new Vector3((float)-Math.Cos(MathHelper.ToRadians(45)), 0, (float)Math.Sin(MathHelper.ToRadians(45)));
            inside_normals[7] = new Vector3((float)Math.Cos(MathHelper.ToRadians(41)), 0, (float)-Math.Sin(MathHelper.ToRadians(41)));

            inside_normals[8] = new Vector3(1, 0, 0);
            inside_normals[9] = new Vector3(-1, 0, 0);

            Vector3 insideDirection1 = new Vector3(-20.888751f, 0, 23.6024f);
            insideDirection1.Normalize();

            insideDirection1 = Vector3.Transform(insideDirection1, this.InitialRotation);

            inside_normals[0].Z = -20.0f;
            inside_normals[0].X = (-20.0f * insideDirection1.Z) / insideDirection1.X;

            inside_normals[0].Normalize();
            inside_normals[1] = -inside_normals[0];

            insidebands_pockets[0] = new OrientedBoundingBox(new Vector3(MIN_X - 9.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z + 18.0f),
                //new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(40.0f)));
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, insideDirection1)));

            Vector3 insideDirection2 = new Vector3(20.888751f, 0, 23.6024f);
            insideDirection2.Normalize();


            insideDirection2 = Vector3.Transform(insideDirection2, this.InitialRotation);

            insidebands_pockets[1] = new OrientedBoundingBox(new Vector3(MIN_X + 19.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 9.5f),
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, insideDirection2)));

            #region MIDDLE'S POCKECTS
            ///////////////////////////
            insidebands_pockets[2] = new OrientedBoundingBox(new Vector3(-20.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 10.0f),
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(-30.0f)));

            insidebands_pockets[3] = new OrientedBoundingBox(new Vector3(20.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 10.0f),
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(30.0f)));
            #endregion

            Vector3 insideDirection3 = new Vector3(20.888751f, 0, -23.6024f);
            insideDirection3.Normalize();
            insideDirection3 = Vector3.Transform(insideDirection3, this.InitialRotation);

            ///////////////////////////
            insidebands_pockets[4] = new OrientedBoundingBox(new Vector3(MAX_X - 18.0f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 10.0f),
                //new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(40.0f)));
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, insideDirection3)));

            insidebands_pockets[5] = new OrientedBoundingBox(new Vector3(MAX_X + 9.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z + 18.0f),
                //new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(40.0f)));
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, insideDirection3)));


            Vector3 insideDirection7 = insideDirection3;
            //insideDirection7.X = -insideDirection7.X;

            //////////////////////
            insidebands_pockets[6] = new OrientedBoundingBox(new Vector3(MAX_X + 10.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z - 20.5f),
                //new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(40.0f)));
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, -insideDirection7)));

            insidebands_pockets[7] = new OrientedBoundingBox(new Vector3(MAX_X - 18.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z + 10.5f),
                //new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(40.0f)));
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY((float)LightManager.AngleBetweenVectors(Vector3.Forward, -insideDirection7)));



            base.Initialize();
        }
    }
}

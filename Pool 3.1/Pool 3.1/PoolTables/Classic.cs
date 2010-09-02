using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.PoolTables
{
    /// <summary>
    /// Classic pool table.
    /// </summary>
    public class Classic : PoolTable
    {
        public Classic(Game _game, string _modelName)
            : base(_game, _modelName)
        {
            CommonConstructor();
        }
        public Classic(Game _game)
            : base(_game, "Models\\classic")
        {
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            MAX_X = 309.73f;
            MIN_X = -309.73f;

            MAX_Z = 153.418f;
            MIN_Z = -153.418f;

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

            headDelimiters = new Vector3[2];
            headDelimiters[0] = new Vector3(MIN_HEAD_X, SURFACE_POSITION_Y + World.ballRadius, MIN_HEAD_Z);
            headDelimiters[1] = new Vector3(MAX_HEAD_X, SURFACE_POSITION_Y + World.ballRadius, MAX_HEAD_Z);
            //headDelimiters[0] = new Vector3(MIN_X + World.ballRadius + float.Epsilon, SURFACE_POSITION_Y + World.ballRadius, MIN_Z + World.ballRadius + float.Epsilon);
            //headDelimiters[1] = new Vector3(MAX_X - World.ballRadius - float.Epsilon, SURFACE_POSITION_Y + World.ballRadius, MAX_Z - World.ballRadius - float.Epsilon);

            cueBallStartPosition = new Vector3(MIN_HEAD_X, SURFACE_POSITION_Y + World.ballRadius, 0.0f);
            cueBallStartLagPositionTeam1 = new Vector3(MIN_HEAD_X, SURFACE_POSITION_Y + World.ballRadius, World.ballRadius * 4.0f);
            cueBallStartLagPositionTeam2 = new Vector3(MIN_HEAD_X, SURFACE_POSITION_Y + World.ballRadius, -World.ballRadius * 4.0f);
            maximumBallsInPocket = 2;
        }

        public override void BuildPockets()
        {
            pockets = new Pocket[6];

            
            const float a = 1.4f;


            pockets[0] = new Pocket(new BoundingSphere(new Vector3(MIN_X - 6.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 6.5f), World.ballRadius * a));
            pockets[0].headpoint = new Vector3(MIN_X, SURFACE_POSITION_Y, MIN_Z);

            pockets[1] = new Pocket(new BoundingSphere(new Vector3(0.0f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 15.3f), World.ballRadius * a));
            pockets[1].headpoint = new Vector3(0.0f, SURFACE_POSITION_Y, MIN_Z);

            pockets[2] = new Pocket(new BoundingSphere(new Vector3(MAX_X + 6.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z - 6.5f), World.ballRadius * a));
            pockets[2].headpoint = new Vector3();

            pockets[3] = new Pocket(new BoundingSphere(new Vector3(MIN_X - 6.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 6.5f), World.ballRadius * a));

            pockets[4] = new Pocket(new BoundingSphere(new Vector3(0, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 15.3f), World.ballRadius * a));

            pockets[5] = new Pocket(new BoundingSphere(new Vector3(MAX_X + 6.5f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z + 6.5f), World.ballRadius * a));


            #region POCKET #0

            pockets[0].insideNormal[0] = new Vector3((float)Math.Cos(MathHelper.ToRadians(38.0f)), 0.0f, (float)-Math.Sin(MathHelper.ToRadians(38.0f)));
            pockets[0].insideNormal[1] = new Vector3((float)Math.Cos(MathHelper.ToRadians(45.0f + 180.0f)), 0.0f, (float)-Math.Sin(MathHelper.ToRadians(45.0f + 180.0f)));

            pockets[0].insideBands[0] = new OrientedBoundingBox(new Vector3(MIN_X - 9.0f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z + 17.5f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 11.75f), Matrix.CreateRotationY(MathHelper.ToRadians(38.0f)));

            pockets[0].insideBands[1] = new OrientedBoundingBox(new Vector3(MIN_X + 18.0f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 10.0f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 11.75f), Matrix.CreateRotationY(MathHelper.ToRadians(45.0f + 180.0f)));


            #endregion


            #region POCKET #1
            pockets[1].insideNormal[0] = new Vector3((float)Math.Cos(MathHelper.ToRadians(30.0f)), 0.0f, (float)Math.Sin(MathHelper.ToRadians(30.0f)));
            pockets[1].insideNormal[1] = new Vector3(-pockets[1].insideNormal[0].X, 0.0f, pockets[1].insideNormal[0].Z);



            pockets[1].insideBands[0] = new OrientedBoundingBox(new Vector3(-21.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 7.5f),
                new Vector3(2, World.ballRadius * 0.5f, 7.0f), Matrix.CreateRotationY(MathHelper.ToRadians(-30.0f)));

            pockets[1].insideBands[1] = new OrientedBoundingBox(new Vector3(21.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 7.5f),
                new Vector3(2, World.ballRadius * 0.5f, 7.0f), Matrix.CreateRotationY(MathHelper.ToRadians(30.0f)));

            #endregion


            #region  POCKET #2





            pockets[2].insideNormal[0] = new Vector3((float)-Math.Cos(MathHelper.ToRadians(48 + 90)), 0, (float)Math.Sin(MathHelper.ToRadians(48 + 90)));
            pockets[2].insideNormal[1] = new Vector3((float)Math.Cos(MathHelper.ToRadians(41 + 90)), 0, (float)-Math.Sin(MathHelper.ToRadians(41 + 90)));

            pockets[2].insideBands[0] = new OrientedBoundingBox(new Vector3(MAX_X - 18.0f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z - 10.0f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 10.0f), Matrix.CreateRotationY(MathHelper.ToRadians(48 + 90)));


            pockets[2].insideBands[1] = new OrientedBoundingBox(new Vector3(MAX_X + 9.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MIN_Z + 18.0f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 10.0f), Matrix.CreateRotationY(MathHelper.ToRadians(41 + 90)));



            #endregion


            #region  POCKET #3

            pockets[3].insideNormal[0] = new Vector3((float)-Math.Cos(MathHelper.ToRadians(48.5f)), 0, (float)Math.Sin(MathHelper.ToRadians(48.5f)));
            pockets[3].insideNormal[1] = new Vector3((float)Math.Cos(MathHelper.ToRadians(49.5f)), 0, (float)-Math.Sin(MathHelper.ToRadians(49.5f)));

            pockets[3].insideBands[0] = new OrientedBoundingBox(new Vector3(MAX_X + 9.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z - 20.5f),
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(48.5f)));

            pockets[3].insideBands[1] = new OrientedBoundingBox(new Vector3(MAX_X - 19.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z + 8.0f),
                new Vector3(2, World.ballRadius * 0.5f, 10), Matrix.CreateRotationY(MathHelper.ToRadians(49.5f)));

            #endregion


            #region  POCKET #4

            pockets[4].insideNormal[0] = new Vector3((float)Math.Cos(MathHelper.ToRadians(30.0f)), 0.0f, (float)-Math.Sin(MathHelper.ToRadians(30.0f)));
            pockets[4].insideNormal[1] = new Vector3(-pockets[4].insideNormal[0].X, 0.0f, pockets[4].insideNormal[0].Z);

            pockets[4].insideBands[0] = new OrientedBoundingBox(new Vector3(-21.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z + 7.05f),
                new Vector3(2, World.ballRadius * 0.5f, 7.0f), Matrix.CreateRotationY(MathHelper.ToRadians(30.0f)));

            pockets[4].insideBands[1] = new OrientedBoundingBox(new Vector3(21.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z + 7.05f),
                new Vector3(2, World.ballRadius * 0.5f, 7.0f), Matrix.CreateRotationY(MathHelper.ToRadians(-30.0f)));

            #endregion


            #region  POCKET #5

            pockets[5].insideNormal[0] = new Vector3((float)Math.Cos(MathHelper.ToRadians(-50.0f)), 0.0f, (float)-Math.Sin(MathHelper.ToRadians(-50.0f)));
            pockets[5].insideNormal[1] = new Vector3(-(float)Math.Cos(MathHelper.ToRadians(-48.0f)), 0.0f, (float)Math.Sin(MathHelper.ToRadians(-48.0f)));


            pockets[5].insideBands[0] = new OrientedBoundingBox(new Vector3(MIN_X - 10.5f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z - 20.0f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 11.75f), Matrix.CreateRotationY(MathHelper.ToRadians(-50.0f)));

            pockets[5].insideBands[1] = new OrientedBoundingBox(new Vector3(MIN_X + 18.0f, SURFACE_POSITION_Y + World.ballRadius * 0.5f, MAX_Z + 9.75f),
                new Vector3(2.0f, World.ballRadius * 0.5f, 11.75f), Matrix.CreateRotationY(MathHelper.ToRadians(-48.0f - 180.0f)));

            #endregion


            for (int i = 0; i < pockets.Length; ++i) pockets[i].SetReady();
        }

        public override void BuildRails()
        {

            ////////// 1 ////////// 2 ///////////
            //   * ---------- * ---------- *   //
            //   |     |                   |   //
            //   |     |                   |   //
            // 0 |     |               *   | 3 //
            //   |     |                   |   //
            //   |     |                   |   //
            //   * ---------- * ---------- *   //
            //////// 5 //////////// 4 ///////////

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
            planes = new Plane[6];
            float railoffset = pocket_radius + 10.2f;

            rails[0] = new BoundingBox(new Vector3(MIN_X, SURFACE_POSITION_Y, MIN_Z + railoffset),
                new Vector3(MIN_X, SURFACE_POSITION_Y + World.ballRadius, MAX_Z - railoffset));

            rails[1] = new BoundingBox(new Vector3(MIN_X + railoffset, SURFACE_POSITION_Y, MIN_Z),
                new Vector3(-railoffset + 2.25f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z));

            rails[2] = new BoundingBox(new Vector3(railoffset - 2.25f, SURFACE_POSITION_Y, MIN_Z),
                new Vector3(MAX_X - railoffset, SURFACE_POSITION_Y + World.ballRadius, MIN_Z));

            rails[3] = new BoundingBox(new Vector3(MAX_X, SURFACE_POSITION_Y, MIN_Z + railoffset),
                new Vector3(MAX_X, SURFACE_POSITION_Y + World.ballRadius, MAX_Z - railoffset));

            rails[4] = new BoundingBox(new Vector3(railoffset - 2.25f, SURFACE_POSITION_Y, MAX_Z),
                new Vector3(MAX_X - railoffset, SURFACE_POSITION_Y + World.ballRadius, MAX_Z)); ;

            rails[5] = new BoundingBox(new Vector3(MIN_X + railoffset, SURFACE_POSITION_Y, MAX_Z),
                new Vector3(-railoffset + 2.25f, SURFACE_POSITION_Y + World.ballRadius, MAX_Z));

            for (int i = 0; i < 6; ++i)
                planes[i] = new Plane(railsNormals[i], -Vector3.Dot((rails[i].Max + railsNormals[i] * World.ballRadius), railsNormals[i]));


            //planes[0] = new Plane(railsNormals[0], -Vector3.Dot((rails[0].Max + railsNormals[0] * World.ballRadius), railsNormals[0]));
            //planes[1] = new Plane(railsNormals[1], -Vector3.Dot((rails[1].Max + railsNormals[1] * World.ballRadius), railsNormals[1]));
            //planes[2] = new Plane(railsNormals[2], -Vector3.Dot((rails[2].Max + railsNormals[2] * World.ballRadius), railsNormals[2]));
            //planes[3] = new Plane(railsNormals[3], -Vector3.Dot((rails[3].Min + railsNormals[3] * World.ballRadius), railsNormals[3]));
            //planes[4] = new Plane(railsNormals[4], -Vector3.Dot((rails[4].Min + railsNormals[4] * World.ballRadius), railsNormals[4]));
            //planes[5] = new Plane(railsNormals[5], -Vector3.Dot((rails[5].Min + railsNormals[5] * World.ballRadius), railsNormals[5]));

            footCushionIndex = 0;
            headCushionIndex = 3;
        }

        public override void Initialize()
        {
            BuildRails();

            ballstuckposition = new Vector3(0, 0, 0);
            BuildPockets();
            
            base.Initialize();
        }
    }
}

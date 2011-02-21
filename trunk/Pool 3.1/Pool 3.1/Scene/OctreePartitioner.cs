//#define DRAW_BOUNDINGBOX
#define DRAW_NORMALS

#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Graphics.Models;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Scenarios;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using XNA_PoolGame.Sticks;
using USHORT = System.UInt16;
#endregion

namespace XNA_PoolGame.Scene
{
    public enum OctreeNodesEnum
    {
        TopLeftFront = 0,
        TopLeftBack = 1,
        TopRightFront = 2,
        TopRightBack = 3,
        BottomLeftFront = 4,
        BottomLeftBack = 5,
        BottomRightFront = 6,
        BottomRightBack = 7,

        Count,
        Root, // DEBUG
    }

    public class GeometryDescription
    {
        public USHORT[] Indices;
        public Vector3[] Vertices;
        public Vector3[] Normals;

        public Vector3[] TriangleNormals;
        public int Triangles;
    }

    public class PartitionedGeometryData
    {
        internal Dictionary<Entity, GeometryDescription> GeometryDescriptions;

        public PartitionedGeometryData()
        {
            GeometryDescriptions = new Dictionary<Entity, GeometryDescription>();
        }

        public void AddModelPart(ref Entity entity, Vector3[] positions, USHORT[] indices, Vector3[] normals, Vector3[] trianglesNormals)
        {
            GeometryDescription geometry;
            if (GeometryDescriptions.TryGetValue(entity, out geometry))
            {
                int IndicesSize = geometry.Indices.Length;
                int VertexSize = geometry.Vertices.Length;
                int NormalSize = geometry.Normals.Length;
                int TriangleNormalsSize = geometry.TriangleNormals.Length;

                Array.Resize<USHORT>(ref geometry.Indices, geometry.Indices.Length + indices.Length);
                Array.Resize<Vector3>(ref geometry.Vertices, geometry.Vertices.Length + positions.Length);
                Array.Resize<Vector3>(ref geometry.Normals, geometry.Normals.Length + normals.Length);
                Array.Resize<Vector3>(ref geometry.TriangleNormals, geometry.TriangleNormals.Length + trianglesNormals.Length);
                geometry.Triangles += indices.Length / 3;


                Array.Copy(indices, 0, geometry.Indices, IndicesSize, indices.Length);
                Array.Copy(positions, 0, geometry.Vertices, VertexSize, positions.Length);
                Array.Copy(normals, 0, geometry.Normals, NormalSize, normals.Length);
                Array.Copy(trianglesNormals, 0, geometry.TriangleNormals, TriangleNormalsSize, trianglesNormals.Length);

                for (int i = IndicesSize; i < geometry.Indices.Length; i++)
                {
                    geometry.Indices[i] += (USHORT)VertexSize;
                }
            }
            else
            {
                geometry = new GeometryDescription();


                geometry.Indices = new USHORT[indices.Length];
                geometry.Vertices = new Vector3[positions.Length];
                geometry.Normals = new Vector3[normals.Length];
                geometry.TriangleNormals = new Vector3[indices.Length / 3];

                Array.Copy(indices, geometry.Indices, indices.Length);
                Array.Copy(positions, geometry.Vertices, positions.Length);
                Array.Copy(normals, geometry.Normals, normals.Length);
                Array.Copy(trianglesNormals, geometry.TriangleNormals, trianglesNormals.Length);

                geometry.Triangles = indices.Length / 3;
                GeometryDescriptions.Add(entity, geometry);
            }
        }
    }

    public class OctreeNode : IEnumerable
    {
        private int currentLevel;

        public int CurrentLevel
        {
            get { return currentLevel; }
            internal set
            {
                currentLevel = value;
            }
        }

        public static Dictionary<OctreeNodesEnum, Color> NodeColors;
        private OctreeNode parent = null;

        public OctreeNode this[OctreeNodesEnum index]
        {
            get
            {
                if ((int)index >= (int)OctreeNodesEnum.Count)
                    throw new ArgumentException("Argumento inválido.");

                switch (index)
                {
                    case OctreeNodesEnum.TopRightFront:
                        return TopRightFrontNode;
                    case OctreeNodesEnum.TopRightBack:
                        return TopRightBackNode;
                    case OctreeNodesEnum.TopLeftFront:
                        return TopLeftFrontNode;
                    case OctreeNodesEnum.TopLeftBack:
                        return TopLeftBackNode;
                    case OctreeNodesEnum.BottomRightFront:
                        return BottomRightFrontNode;
                    case OctreeNodesEnum.BottomRightBack:
                        return BottomRightBackNode;
                    case OctreeNodesEnum.BottomLeftFront:
                        return BottomLeftFrontNode;
                    case OctreeNodesEnum.BottomLeftBack:
                        return BottomLeftBackNode;
                }
                return null;
            }
        }
        internal OctreeNode TopLeftFrontNode;
        internal OctreeNode TopLeftBackNode;
        internal OctreeNode TopRightFrontNode;
        internal OctreeNode TopRightBackNode;

        internal OctreeNode BottomLeftFrontNode;
        internal OctreeNode BottomLeftBackNode;
        internal OctreeNode BottomRightFrontNode;
        internal OctreeNode BottomRightBackNode;

        internal OctreeNodesEnum OctreeChildEnum;
        public int Count
        {
            get
            {
                int count = 0;
                if (TopLeftFrontNode != null)
                    ++count;
                if (TopLeftBackNode != null)
                    ++count;
                if (TopRightFrontNode != null)
                    ++count;
                if (TopRightBackNode != null)
                    ++count;
                if (BottomLeftFrontNode != null)
                    ++count;
                if (BottomLeftBackNode != null)
                    ++count;
                if (BottomRightFrontNode != null)
                    ++count;
                if (BottomRightBackNode != null)
                    ++count;
                return count;
            }
        }
        public Vector3 Center;
        public BoundingBox Box;
        public int Triangles;
        public bool SubDivided { get; internal set; }

        public PartitionedGeometryData PGD;
        public OctreeNode(OctreeNode parent, OctreeNodesEnum OctreeChildEnum)
        {
            this.parent = parent;
            this.OctreeChildEnum = OctreeChildEnum;
            SubDivided = false;

            if (NodeColors == null)
            {
                NodeColors = new Dictionary<OctreeNodesEnum, Color>();
                NodeColors.Add(OctreeNodesEnum.BottomLeftBack, Color.Aqua);
                NodeColors.Add(OctreeNodesEnum.BottomLeftFront, Color.Beige);
                NodeColors.Add(OctreeNodesEnum.BottomRightBack, Color.BlanchedAlmond);
                NodeColors.Add(OctreeNodesEnum.BottomRightFront, Color.DarkGray);
                NodeColors.Add(OctreeNodesEnum.TopLeftBack, Color.Orange);
                NodeColors.Add(OctreeNodesEnum.TopLeftFront, Color.Blue);
                NodeColors.Add(OctreeNodesEnum.TopRightBack, Color.Yellow);
                NodeColors.Add(OctreeNodesEnum.TopRightFront, Color.White);

                NodeColors.Add(OctreeNodesEnum.Root, Color.Red);
            }
        }

        public void DrawBoundingBox()
        {
            //if (debug)
            //{
            //    World.poolTable.vectorRenderer.SetWorldMatrix(Matrix.Identity);
            //    World.poolTable.vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            //    World.poolTable.vectorRenderer.SetColor(NodeColors[OctreeChildEnum]);

            //    PoolGame.device.RenderState.DepthBufferWriteEnable = false;
            //    PoolGame.device.RenderState.DepthBufferEnable = false;
            //    World.poolTable.vectorRenderer.DrawBoundingBox(Box);
            //}
#if DRAW_NORMALS
            if (PGD == null) return;

            World.poolTable.vectorRenderer.SetWorldMatrix(Matrix.Identity);
            World.poolTable.vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            // Normals
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;
            PoolGame.device.RenderState.DepthBufferEnable = true;

      
            World.poolTable.vectorRenderer.SetColor(Color.LightPink);
            foreach (KeyValuePair<Entity, GeometryDescription> item in PGD.GeometryDescriptions)
            {
                GeometryDescription geometry = item.Value;
                for (int i = 0; i < geometry.Triangles; i++)
                {
                    Vector3 v0 = geometry.Vertices[geometry.Indices[i * 3 + 0]];
                    Vector3 v1 = geometry.Vertices[geometry.Indices[i * 3 + 1]];
                    Vector3 v2 = geometry.Vertices[geometry.Indices[i * 3 + 2]];

                    Vector3 center = (v0 + v1 + v2) / 3f;

                    World.poolTable.vectorRenderer.DrawLine(center, center + geometry.TriangleNormals[i] * 20f);        
                }
            }
#endif
        }

        #region Miembros de IEnumerable

        public IEnumerator GetEnumerator()
        {
            if (TopLeftFrontNode != null)
                yield return TopLeftFrontNode;
            if (TopLeftBackNode != null)
                yield return TopLeftBackNode;
            if (TopRightFrontNode != null)
                yield return TopRightFrontNode;
            if (TopRightBackNode != null)
                yield return TopRightBackNode;
            if (BottomLeftFrontNode != null)
                yield return BottomLeftFrontNode;
            if (BottomLeftBackNode != null)
                yield return BottomLeftBackNode;
            if (BottomRightFrontNode != null)
                yield return BottomRightFrontNode;
            if (BottomRightBackNode != null)
                yield return BottomRightBackNode;
            
        }

        #endregion
    }

    public class OctreePartitioner : SceneManager
    {
        public OctreeNode Root { get { return root; } private set { root = value; } }
        private OctreeNode root;
        private readonly int MaxSubdivisions;
        private readonly int MaxTriangles;
        private int CurrentSubdivision;

        public int MaxNodeLevel { get; private set; }

        public OctreePartitioner(Scenario Scenario, int MaxLevels)
            : base(Scenario)
        {
            Root = new OctreeNode(null, OctreeNodesEnum.Root);
            Root.CurrentLevel = 0;
            this.MaxSubdivisions = MaxLevels;
            this.MaxTriangles = 1000;
            CurrentSubdivision = 0;
            MaxNodeLevel = 0;

            collider = new OctreeCollider(this);
        }
        
        public override void DrawScene(GameTime gameTime)
        {
            totalItemDrawn = 0;
            Camera activeCamera = World.camera;


            if (activeCamera.EnableFrustumCulling)
            {
                if (activeCamera.Frustum.Contains(Root.Box) != ContainmentType.Disjoint)
                    DrawBoundingBoxes(Root, activeCamera);
            }
            else
                DrawBoundingBoxes(Root, activeCamera);


        }

        private void DrawBoundingBoxes(OctreeNode node, Camera activeCamera)
        {
            if (node == null)
                return;

            if (node.SubDivided)
            {
                foreach (OctreeNode child in node)
                {
                    if (child != null)
                    {
                        if (activeCamera.EnableFrustumCulling)
                            if (activeCamera.Frustum.Contains(child.Box) == ContainmentType.Disjoint)
                                continue;

                        DrawBoundingBoxes(child, activeCamera);
                        child.DrawBoundingBox();
                    }
                }
            }
            else
            {
                node.DrawBoundingBox();
                totalItemDrawn++;
            }
        }

        public override void BuildScene()
        {
            BoundingBox Box;
            Box.Min = new Vector3(float.MaxValue);
            Box.Max = new Vector3(float.MinValue);
            foreach (DrawableComponent item in scenario.Objects)
            {
                Entity entity = item as Entity;
                if (entity != null && !(entity is Ball) && !(entity is Stick) && entity.Visible)
                {
                    int i = 0;
                    foreach (CustomModelPart modelPart in entity.ModelL1.modelParts)
                    {
                        Drawn[modelPart] = false;
                        
                        Box = BoundingBox.CreateMerged(Box, entity.Boxes[i]);
                        ++i;
                    }
                }
            }

            totalItems = 0;
            MaxNodeLevel = 0;
            PartitionedGeometryData pgd = new PartitionedGeometryData();
            GetSceneGeometryDesc(ref pgd);

            Root.Center = (Box.Max + Box.Min) / 2.0f;
            Vector3 Size = (Box.Max - Box.Min);

            float maxSize = Math.Max(Math.Max(Size.X, Size.Y), Size.Z);
            //Size = new Vector3(maxSize);

            Root.Box.Min = Root.Center - Size / 2f;
            Root.Box.Max = Root.Center + Size / 2f;

            int triangles = GetTrianglesCount();
            Build(ref pgd, ref root, triangles, Size, Root.Center);
        }

        private void Build(ref PartitionedGeometryData pgd, ref OctreeNode node, int numTriangles, Vector3 nodeSize, Vector3 nodeCenter)
        {
            node.Center = nodeCenter;
            node.CurrentLevel = CurrentSubdivision;
            node.Box.Max = nodeCenter + nodeSize / 2f;
            node.Box.Min = nodeCenter - nodeSize / 2f;

            MaxNodeLevel = Math.Max(CurrentSubdivision, MaxNodeLevel);
            if (numTriangles > MaxTriangles && CurrentSubdivision < MaxSubdivisions)
            {
                node.SubDivided = true;

                // Por cada entity, un booleano que indica si un triángulo pertenece a cualquiera de los 8 nodos.
                // 
                Dictionary<Entity, TriangleList> TopLeftFrontList = new Dictionary<Entity, TriangleList>(); // TopLeftFront
                Dictionary<Entity, TriangleList> TopLeftBackList = new Dictionary<Entity, TriangleList>(); // TopLeftBack
                Dictionary<Entity, TriangleList> TopRightBackList = new Dictionary<Entity, TriangleList>(); // TopRightBack
                Dictionary<Entity, TriangleList> TopRightFrontList = new Dictionary<Entity, TriangleList>(); // TopRightFront
                Dictionary<Entity, TriangleList> BottomLeftFrontList = new Dictionary<Entity, TriangleList>(); // BottomLeftFront
                Dictionary<Entity, TriangleList> BottomLeftBackList = new Dictionary<Entity, TriangleList>(); // BottomLeftBack
                Dictionary<Entity, TriangleList> BottomRightBackList = new Dictionary<Entity, TriangleList>(); // BottomRightBack
                Dictionary<Entity, TriangleList> BottomRightFrontList = new Dictionary<Entity, TriangleList>(); // BottomRightFront

                foreach (KeyValuePair<Entity, GeometryDescription> item in pgd.GeometryDescriptions)
                {
                    GeometryDescription geometry = item.Value;

                    for (int j = 0; j < geometry.Indices.Length; ++j)
                    {
                        Vector3 point = geometry.Vertices[geometry.Indices[j]];

                        // TopLeftFront
                        if ((point.X <= node.Center.X) && (point.Y >= node.Center.Y) && (point.Z >= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref TopLeftFrontList, item.Key, j / 3);

                        // TopLeftBack
                        if ((point.X <= node.Center.X) && (point.Y >= node.Center.Y) && (point.Z <= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref TopLeftBackList, item.Key, j / 3);

                        // TopRightBack
                        if ((point.X >= node.Center.X) && (point.Y >= node.Center.Y) && (point.Z <= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref TopRightBackList, item.Key, j / 3);

                        // TopRightFront
                        if ((point.X >= node.Center.X) && (point.Y >= node.Center.Y) && (point.Z >= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref TopRightFrontList, item.Key, j / 3);

                        // BottomLeftFront
                        if ((point.X <= node.Center.X) && (point.Y <= node.Center.Y) && (point.Z >= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref BottomLeftFrontList, item.Key, j / 3);

                        // BottomLeftBack
                        if ((point.X <= node.Center.X) && (point.Y <= node.Center.Y) && (point.Z <= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref BottomLeftBackList, item.Key, j / 3);

                        // BottomRightBack
                        if ((point.X >= node.Center.X) && (point.Y <= node.Center.Y) && (point.Z <= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref BottomRightBackList, item.Key, j / 3);

                        // BottomRightFront
                        if ((point.X >= node.Center.X) && (point.Y <= node.Center.Y) && (point.Z >= node.Center.Z))
                            AddEntityAndTriangleToListNodeHelper(ref pgd, ref BottomRightFrontList, item.Key, j / 3);
                    }
                }

                int triCount1 = 0;
                int triCount2 = 0;
                int triCount3 = 0;
                int triCount4 = 0;
                int triCount5 = 0;
                int triCount6 = 0;
                int triCount7 = 0;
                int triCount8 = 0;

                GetFaceListTriangleCount(ref TopLeftFrontList, out triCount1);
                GetFaceListTriangleCount(ref TopLeftBackList, out triCount2);
                GetFaceListTriangleCount(ref TopRightBackList, out triCount3);
                GetFaceListTriangleCount(ref TopRightFrontList, out triCount4);
                GetFaceListTriangleCount(ref BottomLeftFrontList, out triCount5);
                GetFaceListTriangleCount(ref BottomLeftBackList, out triCount6);
                GetFaceListTriangleCount(ref BottomRightBackList, out triCount7);
                GetFaceListTriangleCount(ref BottomRightFrontList, out triCount8);

                CreateNewNode(ref pgd, ref TopLeftFrontList, triCount1, node.Center, nodeSize, ref node, OctreeNodesEnum.TopLeftFront);
                CreateNewNode(ref pgd, ref TopLeftBackList, triCount2, node.Center, nodeSize, ref node, OctreeNodesEnum.TopLeftBack);
                CreateNewNode(ref pgd, ref TopRightBackList, triCount3, node.Center, nodeSize, ref node, OctreeNodesEnum.TopRightBack);
                CreateNewNode(ref pgd, ref TopRightFrontList, triCount4, node.Center, nodeSize, ref node, OctreeNodesEnum.TopRightFront);
                CreateNewNode(ref pgd, ref BottomLeftFrontList, triCount5, node.Center, nodeSize, ref node, OctreeNodesEnum.BottomLeftFront);
                CreateNewNode(ref pgd, ref BottomLeftBackList, triCount6, node.Center, nodeSize, ref node, OctreeNodesEnum.BottomLeftBack);
                CreateNewNode(ref pgd, ref BottomRightBackList, triCount7, node.Center, nodeSize, ref node, OctreeNodesEnum.BottomRightBack);
                CreateNewNode(ref pgd, ref BottomRightFrontList, triCount8, node.Center, nodeSize, ref node, OctreeNodesEnum.BottomRightFront);
            }
            else
            {
                node.SubDivided = false;
                node.Triangles = numTriangles;

                node.PGD = pgd;
                ++totalItems;
            }
        }

        #region Create New Node
        private void CreateNewNode(ref PartitionedGeometryData pgd, ref Dictionary<Entity, TriangleList> NodeTriangleList, int TriangleCount, Vector3 nodeCenter, Vector3 nodeSize, ref OctreeNode node, OctreeNodesEnum nodeEnum)
        {
            if (TriangleCount <= 0)
                return;

            PartitionedGeometryData tmpPGD = new PartitionedGeometryData();
            foreach (KeyValuePair<Entity, TriangleList> item in NodeTriangleList)
            {
                Entity entity = item.Key;
                TriangleList list = item.Value;

                GeometryDescription newGeometry = new GeometryDescription();
                newGeometry.Indices = new USHORT[list.TriangleCount * 3];
                newGeometry.TriangleNormals = new Vector3[list.TriangleCount];

                int index = 0;
                for (int i = 0; i < list.Triangles.Count; i++)
                {
                    if (list.Triangles[i])
                    {
                        newGeometry.Indices[index * 3] = pgd.GeometryDescriptions[entity].Indices[i * 3];
                        newGeometry.Indices[index * 3 + 1] = pgd.GeometryDescriptions[entity].Indices[i * 3 + 1];
                        newGeometry.Indices[index * 3 + 2] = pgd.GeometryDescriptions[entity].Indices[i * 3 + 2];

                        //Vector3 v1 = pgd.GeometryDescriptions[entity].Vertices[pgd.GeometryDescriptions[entity].Indices[i * 3]]
                        //    - pgd.GeometryDescriptions[entity].Vertices[pgd.GeometryDescriptions[entity].Indices[i * 3 + 1]];

                        //Vector3 v2 = pgd.GeometryDescriptions[entity].Vertices[pgd.GeometryDescriptions[entity].Indices[i * 3]]
                        //    - pgd.GeometryDescriptions[entity].Vertices[pgd.GeometryDescriptions[entity].Indices[i * 3 + 2]];

                        //newGeometry.TriangleNormals[index] = Vector3.Cross(v2, v1);
                        //newGeometry.TriangleNormals[index].Normalize();

                        newGeometry.TriangleNormals[index] = pgd.GeometryDescriptions[entity].TriangleNormals[i];
                        newGeometry.Triangles++;
                        index++;
                    }
                }
                //newGeometry.Vertices = new Vector3[pgd.GeometryDescriptions[entity].Vertices.Length];
                //Array.Copy(pgd.GeometryDescriptions[entity].Vertices, newGeometry.Vertices, pgd.GeometryDescriptions[entity].Vertices.Length);
                newGeometry.Vertices = pgd.GeometryDescriptions[entity].Vertices;
                newGeometry.Normals = pgd.GeometryDescriptions[entity].Normals;


                //List<Vector3> tmpVertices = new List<Vector3>();
                //int index = 0;
                //Dictionary<int, bool> markVertices = new Dictionary<int, bool>();
                //for (int i = 0; i < pgd.GeometryDescriptions[entity].Vertices.Length; i++) markVertices[i] = false;

                //int baseVertexIndex = int.MaxValue;
                //Dictionary<int, int> markIndices = new Dictionary<int, int>();
                //for (int i = 0; i < list.TriangleCount * 3; i++) markIndices[i] = 0;
                //for (int i = 0; i < list.Triangles.Count; i++)
                //{
                //    if (list.Triangles[i])
                //    {
                //        int indice1 = pgd.GeometryDescriptions[entity].Indices[i * 3];
                //        int indice2 = pgd.GeometryDescriptions[entity].Indices[i * 3 + 1];
                //        int indice3 = pgd.GeometryDescriptions[entity].Indices[i * 3 + 2];
                //        baseVertexIndex = Math.Min(Math.Min(Math.Min(indice1, indice2), indice3), baseVertexIndex);

                //        markVertices[indice1] = true;
                //        markVertices[indice2] = true;
                //        markVertices[indice3] = true;

                //        markIndices[indice1] = -1;
                //        markIndices[indice2] = -1;
                //        markIndices[indice3] = -1;
                //        ++index;
                //    }
                //}

                //markIndices[baseVertexIndex] = 0;
                //index = 0;
                //for (int i = 0; i < list.Triangles.Count; i++)
                //{
                //    if (list.Triangles[i])
                //    {
                //        int indice1 = pgd.GeometryDescriptions[entity].Indices[i * 3];
                //        int indice2 = pgd.GeometryDescriptions[entity].Indices[i * 3 + 1];
                //        int indice3 = pgd.GeometryDescriptions[entity].Indices[i * 3 + 2];

                //        int i1 = GetIndexHelper(ref markIndices, indice1);
                //        int i2 = GetIndexHelper(ref markIndices, indice2);
                //        int i3 = GetIndexHelper(ref markIndices, indice3);

                //        newGeometry.Indices[index * 3] = (short)i1;
                //        newGeometry.Indices[index * 3 + 1] = (short)i2;
                //        newGeometry.Indices[index * 3 + 2] = (short)i3;

                //        //newGeometry.Indices[index * 3] = pgd.GeometryDescriptions[entity].Indices[i * 3];
                //        //newGeometry.Indices[index * 3 + 1] = pgd.GeometryDescriptions[entity].Indices[i * 3 + 1];
                //        //newGeometry.Indices[index * 3 + 2] = pgd.GeometryDescriptions[entity].Indices[i * 3 + 2];
                //        newGeometry.Triangles++;

                //        ++index;
                //    }
                //}

                //int newVerticesCount = 0;
                //for (int i = 0; i < markVertices.Count; i++)
                //{
                //    if (markVertices[i])
                //        newVerticesCount++;
                //}
                //newGeometry.Vertices = new Vector3[newVerticesCount];
                //index = 0;
                //for (int i = 0; i < markVertices.Count; i++)
                //{
                //    if (markVertices[i])
                //    {
                //        newGeometry.Vertices[index] = pgd.GeometryDescriptions[entity].Vertices[i];
                //        ++index;
                //    }
                //}
                tmpPGD.GeometryDescriptions.Add(entity, newGeometry);
            }

            Vector3 newNodeCenter = Vector3.Zero;
            ++CurrentSubdivision;
            switch (nodeEnum)
            {
                case OctreeNodesEnum.TopLeftFront:
                    newNodeCenter = new Vector3(nodeCenter.X - nodeSize.X / 4f, nodeCenter.Y + nodeSize.Y / 4f, nodeCenter.Z + nodeSize.Z / 4f);
                    node.TopLeftFrontNode = new OctreeNode(node, OctreeNodesEnum.TopLeftFront);
                    Build(ref tmpPGD, ref node.TopLeftFrontNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.TopLeftBack:
                    newNodeCenter = new Vector3(nodeCenter.X - nodeSize.X / 4f, nodeCenter.Y + nodeSize.Y / 4f, nodeCenter.Z - nodeSize.Z / 4f);
                    node.TopLeftBackNode = new OctreeNode(node, OctreeNodesEnum.TopLeftBack);
                    Build(ref tmpPGD, ref node.TopLeftBackNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.TopRightBack:
                    newNodeCenter = new Vector3(nodeCenter.X + nodeSize.X / 4f, nodeCenter.Y + nodeSize.Y / 4f, nodeCenter.Z - nodeSize.Z / 4f);
                    node.TopRightBackNode = new OctreeNode(node, OctreeNodesEnum.TopRightBack);
                    Build(ref tmpPGD, ref node.TopRightBackNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.TopRightFront:
                    newNodeCenter = new Vector3(nodeCenter.X + nodeSize.X / 4f, nodeCenter.Y + nodeSize.Y / 4f, nodeCenter.Z + nodeSize.Z / 4f);
                    node.TopRightFrontNode = new OctreeNode(node, OctreeNodesEnum.TopRightFront);
                    Build(ref tmpPGD, ref node.TopRightFrontNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.BottomLeftFront:
                    newNodeCenter = new Vector3(nodeCenter.X - nodeSize.X / 4f, nodeCenter.Y - nodeSize.Y / 4f, nodeCenter.Z + nodeSize.Z / 4f);
                    node.BottomLeftFrontNode = new OctreeNode(node, OctreeNodesEnum.BottomLeftFront);
                    Build(ref tmpPGD, ref node.BottomLeftFrontNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.BottomLeftBack:
                    newNodeCenter = new Vector3(nodeCenter.X - nodeSize.X / 4f, nodeCenter.Y - nodeSize.Y / 4f, nodeCenter.Z - nodeSize.Z / 4f);
                    node.BottomLeftBackNode = new OctreeNode(node, OctreeNodesEnum.BottomLeftBack);
                    Build(ref tmpPGD, ref node.BottomLeftBackNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;


                case OctreeNodesEnum.BottomRightBack:
                    newNodeCenter = new Vector3(nodeCenter.X + nodeSize.X / 4f, nodeCenter.Y - nodeSize.Y / 4f, nodeCenter.Z - nodeSize.Z / 4f);
                    node.BottomRightBackNode = new OctreeNode(node, OctreeNodesEnum.BottomRightBack);
                    Build(ref tmpPGD, ref node.BottomRightBackNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;

                case OctreeNodesEnum.BottomRightFront:
                    newNodeCenter = new Vector3(nodeCenter.X + nodeSize.X / 4f, nodeCenter.Y - nodeSize.Y / 4f, nodeCenter.Z + nodeSize.Z / 4f);
                    node.BottomRightFrontNode = new OctreeNode(node, OctreeNodesEnum.BottomRightFront);
                    Build(ref tmpPGD, ref node.BottomRightFrontNode, TriangleCount, nodeSize / 2f, newNodeCenter);
                    break;
            }

            --CurrentSubdivision;
        }
        #endregion

        private int GetIndexHelper(ref Dictionary<int, int> indices, int tofind)
        {
            int index = 0;
            foreach (KeyValuePair<int, int> item in indices)
            {
                if (item.Key == tofind)
                    return index;

                index++;
            }
            return 0;
        }

        private void GetFaceListTriangleCount(ref Dictionary<Entity, TriangleList> list, out int TriangleNodeCount)
        {
            int tmptriangleListCount = 0;
            foreach (KeyValuePair<Entity, TriangleList> item in list)
            {
                Entity entity = item.Key;
                TriangleList face = item.Value;
                int tmpFaceCount = 0;
                for (int i = 0; i < face.Triangles.Count; i++)
                {
                    if (face.Triangles[i])
                    {
                        ++tmpFaceCount;
                        ++tmptriangleListCount;
                    }
                }

                list[entity].TriangleCount = tmpFaceCount;
            }
            TriangleNodeCount = tmptriangleListCount;
        }

        private class TriangleList
        {
            public List<bool> Triangles;
            public int TriangleCount;
            public TriangleList() { }
        }

        #region Triangle Count
        private int GetTrianglesCount()
        {
            int triangles = 0;
            foreach (DrawableComponent item in scenario.Objects)
            {
                Entity entity = item as Entity;
                if (entity != null && !(entity is Ball) && !(entity is Stick) && entity.Visible)
                {
                    foreach (CustomModelPart modelPart in entity.ModelL1.modelParts)
                    {
                        triangles += modelPart.TriangleCount;
                    }
                }
            }

            return triangles;
        }
        #endregion

        public struct VertexPositionNormalTextureBinormalTangent
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoord;
            public Vector3 Binormal;
            public Vector3 Tangent;

            //public static VertexElement[] VertexElements;
        }
        private void GetSceneGeometryDesc(ref PartitionedGeometryData partitionedData)
        {
            foreach (DrawableComponent item in scenario.Objects)
            {
                Entity entity = item as Entity;
                if (entity != null && !(entity is Ball) && !(entity is Stick) && entity.Visible)
                {
                    // Para cada CustomModelPart, agrega la información de la geometría.
                    foreach (CustomModelPart modelPart in entity.ModelL1.modelParts)
                    {
                        //VertexElement[] VertexElements = modelPart.VertexDeclaration.GetVertexElements();

                        Vector3[] localpositions = new Vector3[modelPart.VertexCount];
                        Vector3[] localnormals = new Vector3[modelPart.VertexCount];

                        VertexPositionNormalTextureBinormalTangent[] partInfo = new VertexPositionNormalTextureBinormalTangent[modelPart.VertexCount];

                        modelPart.VertexBuffer.GetData(partInfo);

                        for (int i = 0; i < modelPart.VertexCount; ++i)
                        {
                            // Aplica la transformación que tiene el entity a todos los vértices.
                            localpositions[i] = Vector3.Transform(partInfo[i].Position, entity.LocalWorld);

                            Vector3 scale, translation;
                            Quaternion rotation;
                            entity.LocalWorld.Decompose(out scale, out rotation, out translation);
                            localnormals[i] = Vector3.Transform(partInfo[i].Normal, rotation);
                            localnormals[i].Normalize();
                        }


                        int bytes = modelPart.IndexBuffer.SizeInBytes / (modelPart.TriangleCount * 3);
                        USHORT[] localindices = new USHORT[modelPart.TriangleCount * 3];
                        modelPart.IndexBuffer.GetData(localindices);

                        Vector3[] trianglesNormals = new Vector3[modelPart.TriangleCount];
                        for (int i = 0; i < modelPart.TriangleCount; i++)
                        {
                            Vector3 v1 = localpositions[localindices[i * 3]]
                            - localpositions[localindices[i * 3 + 1]];

                            Vector3 v2 = localpositions[localindices[i * 3]]
                            - localpositions[localindices[i * 3 + 2]];

                            trianglesNormals[i] = Vector3.Cross(v2, v1);
                            trianglesNormals[i].Normalize();
                        }

                        //Vector3 vv1 = Vector3.Right;
                        //Vector3 vv2 = Vector3.Up;

                        //Vector3 n = Vector3.Cross(vv1, vv2);
                        partitionedData.AddModelPart(ref entity, localpositions, localindices, localnormals, trianglesNormals);
                    }
                }
            }

        }

        private void AddEntityAndTriangleToListNodeHelper(ref PartitionedGeometryData pgd, ref Dictionary<Entity, TriangleList> list, Entity entity, int index)
        {
            TriangleList facelist;
            if (list.TryGetValue(entity, out facelist))
            {
                facelist.Triangles[index] = true;
            }
            else
            {
                GeometryDescription geometry;
                pgd.GeometryDescriptions.TryGetValue(entity, out geometry);
                
                facelist = new TriangleList();
                facelist.Triangles = new List<bool>(geometry.Triangles);
                for (int i = 0; i < geometry.Triangles; ++i) facelist.Triangles.Add(false);

                facelist.Triangles[index] = true;

                facelist.TriangleCount = 0;
                list.Add(entity, facelist);
            }
        }
        
    }
}

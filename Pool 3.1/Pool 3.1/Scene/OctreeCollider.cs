using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Models;

namespace XNA_PoolGame.Scene
{
    public class OctreeCollider
    {
        OctreePartitioner partitioner;
        public int SubdivisionReached { get; private set; }
        public OctreeCollider(OctreePartitioner partitioner)
        {
            this.partitioner = partitioner;
        }

        public void CheckCollisions(BoundingSphere sphere, ref List<int> collidingFaces, ref List<Vector3> collisionPoints)
        {
            Queue<OctreeNode> queue = new Queue<OctreeNode>();
            queue.Enqueue(partitioner.Root);
            SubdivisionReached = 0;

            while (queue.Count > 0)
            {
                OctreeNode node = queue.Dequeue();
                SubdivisionReached = Math.Max(node.CurrentLevel, SubdivisionReached);

                if (node.SubDivided)
                {
                    // TopLeftFront
                    if ((sphere.Center.X <= node.Center.X) && (sphere.Center.Y >= node.Center.Y) && (sphere.Center.Z >= node.Center.Z) && node.TopLeftFrontNode != null)
                        queue.Enqueue(node.TopLeftFrontNode);

                    // TopLeftBack
                    if ((sphere.Center.X <= node.Center.X) && (sphere.Center.Y >= node.Center.Y) && (sphere.Center.Z <= node.Center.Z) && node.TopLeftBackNode != null)
                        queue.Enqueue(node.TopLeftBackNode);

                    // TopRightBack
                    if ((sphere.Center.X >= node.Center.X) && (sphere.Center.Y >= node.Center.Y) && (sphere.Center.Z <= node.Center.Z) && node.TopRightBackNode != null)
                        queue.Enqueue(node.TopRightBackNode);

                    // TopRightFront
                    if ((sphere.Center.X >= node.Center.X) && (sphere.Center.Y >= node.Center.Y) && (sphere.Center.Z >= node.Center.Z) && node.TopRightFrontNode != null)
                        queue.Enqueue(node.TopRightFrontNode);

                    // BottomLeftFront
                    if ((sphere.Center.X <= node.Center.X) && (sphere.Center.Y <= node.Center.Y) && (sphere.Center.Z >= node.Center.Z) && node.BottomLeftFrontNode != null)
                        queue.Enqueue(node.BottomLeftFrontNode);

                    // BottomLeftBack
                    if ((sphere.Center.X <= node.Center.X) && (sphere.Center.Y <= node.Center.Y) && (sphere.Center.Z <= node.Center.Z) && node.BottomLeftBackNode != null)
                        queue.Enqueue(node.BottomLeftBackNode);

                    // BottomRightBack
                    if ((sphere.Center.X >= node.Center.X) && (sphere.Center.Y <= node.Center.Y) && (sphere.Center.Z <= node.Center.Z) && node.BottomRightBackNode != null)
                        queue.Enqueue(node.BottomRightBackNode);

                    // BottomRightFront
                    if ((sphere.Center.X >= node.Center.X) && (sphere.Center.Y <= node.Center.Y) && (sphere.Center.Z >= node.Center.Z) && node.BottomRightFrontNode != null)
                        queue.Enqueue(node.BottomRightFrontNode);
                }
                else
                {
                    float r2 = sphere.Radius * sphere.Radius;
                    foreach (KeyValuePair<Entity, GeometryDescription> item in node.PGD.GeometryDescriptions)
                    {
                        GeometryDescription geometry = item.Value;
                        for (int i = 2; i < geometry.Triangles; i++)
                        {
                            Vector3 pos1 = geometry.Vertices[geometry.Indices[i * 3]];
                            Vector3 pos2 = geometry.Vertices[geometry.Indices[i * 3 + 1]];
                            Vector3 pos3 = geometry.Vertices[geometry.Indices[i * 3 + 2]];

                            BoundingBox box;
                            box.Min = Vector3.Min(pos1, Vector3.Min(pos2, pos3));
                            box.Max = Vector3.Max(pos1, Vector3.Max(pos2, pos3));

                            if (sphere.Contains(box) == ContainmentType.Disjoint)
                                continue;

                            Vector3 closestPoint = closestPointInTriangle(sphere.Center, pos1, pos2, pos3);
                            
                            float squaredDist = (closestPoint - sphere.Center).LengthSquared();
                            if (squaredDist <= r2)
                            {
                                // TODO: Cambiar esto, está malo. Debería tener una referencia al triángulo.
                                collidingFaces.Add(i);
                                collisionPoints.Add(closestPoint);
                            }
                        }
                    } 
                }
            }
        }

        /// <summary>
        /// Computes the distance between point and the triangle (v0, v1, v2). 
        /// Code taken from http://www.geometrictools.com/LibFoundation/Distance/Distance.html
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private Vector3 closestPointInTriangle(Vector3 point, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            {
                //  Vector3<Real> kDiff = m_rkTriangle.V[0] - m_rkVector;
                Vector3 kDiff = v0 - point;
                Vector3 kEdge0 = v1 - v0;
                Vector3 kEdge1 = v2 - v0;

                double fA00 = kEdge0.LengthSquared();

                double fA01 = Vector3.Dot(kEdge0, kEdge1);
                double fA11 = kEdge1.LengthSquared();
                double fB0 = Vector3.Dot(kDiff, kEdge0);
                double fB1 = Vector3.Dot(kDiff, kEdge1);
                double fC = kDiff.LengthSquared();
                double fDet = Math.Abs(fA00 * fA11 - fA01 * fA01);
                double fS = fA01 * fB1 - fA11 * fB0;
                double fT = fA01 * fB0 - fA00 * fB1;
                double fSqrDistance;

                if (fS + fT <= fDet)
                {
                    if (fS < 0.0)
                    {
                        if (fT < 0.0)  // region 4
                        {
                            if (fB0 < 0.0)
                            {
                                fT = 0.0;
                                if (-fB0 >= fA00)
                                {
                                    fS = 1.0;
                                    fSqrDistance = fA00 + 2.0 * fB0 + fC;
                                }
                                else
                                {
                                    fS = -fB0 / fA00;
                                    fSqrDistance = fB0 * fS + fC;
                                }
                            }
                            else
                            {
                                fS = 0.0;
                                if (fB1 >= 0.0)
                                {
                                    fT = 0.0;
                                    fSqrDistance = fC;
                                }
                                else if (-fB1 >= fA11)
                                {
                                    fT = 1.0;
                                    fSqrDistance = fA11 + 2.0 * fB1 + fC;
                                }
                                else
                                {
                                    fT = -fB1 / fA11;
                                    fSqrDistance = fB1 * fT + fC;
                                }
                            }
                        }
                        else  // region 3
                        {
                            fS = 0.0;
                            if (fB1 >= 0.0)
                            {
                                fT = 0.0;
                                fSqrDistance = fC;
                            }
                            else if (-fB1 >= fA11)
                            {
                                fT = 1.0;
                                fSqrDistance = fA11 + 2.0 * fB1 + fC;
                            }
                            else
                            {
                                fT = -fB1 / fA11;
                                fSqrDistance = fB1 * fT + fC;
                            }
                        }
                    }
                    else if (fT < 0.0)  // region 5
                    {
                        fT = 0.0;
                        if (fB0 >= 0.0)
                        {
                            fS = 0.0;
                            fSqrDistance = fC;
                        }
                        else if (-fB0 >= fA00)
                        {
                            fS = 1.0;
                            fSqrDistance = fA00 + 2.0 * fB0 + fC;
                        }
                        else
                        {
                            fS = -fB0 / fA00;
                            fSqrDistance = fB0 * fS + fC;
                        }
                    }
                    else  // region 0
                    {
                        // minimum at interior point
                        double fInvDet = 1.0 / fDet;
                        fS *= fInvDet;
                        fT *= fInvDet;
                        fSqrDistance = fS * (fA00 * fS + fA01 * fT + 2.0 * fB0) +
                            fT * (fA01 * fS + fA11 * fT + 2.0 * fB1) + fC;
                    }
                }
                else
                {
                    double fTmp0, fTmp1, fNumer, fDenom;

                    if (fS < 0.0)  // region 2
                    {
                        fTmp0 = fA01 + fB0;
                        fTmp1 = fA11 + fB1;
                        if (fTmp1 > fTmp0)
                        {
                            fNumer = fTmp1 - fTmp0;
                            fDenom = fA00 - 2.0f * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fS = 1.0;
                                fT = 0.0;
                                fSqrDistance = fA00 + 2.0 * fB0 + fC;
                            }
                            else
                            {
                                fS = fNumer / fDenom;
                                fT = 1.0 - fS;
                                fSqrDistance = fS * (fA00 * fS + fA01 * fT + 2.0f * fB0) +
                                    fT * (fA01 * fS + fA11 * fT + 2.0) * fB1 + fC;
                            }
                        }
                        else
                        {
                            fS = 0.0;
                            if (fTmp1 <= 0.0)
                            {
                                fT = 1.0;
                                fSqrDistance = fA11 + 2.0 * fB1 + fC;
                            }
                            else if (fB1 >= 0.0)
                            {
                                fT = 0.0;
                                fSqrDistance = fC;
                            }
                            else
                            {
                                fT = -fB1 / fA11;
                                fSqrDistance = fB1 * fT + fC;
                            }
                        }
                    }
                    else if (fT < 0.0)  // region 6
                    {
                        fTmp0 = fA01 + fB1;
                        fTmp1 = fA00 + fB0;
                        if (fTmp1 > fTmp0)
                        {
                            fNumer = fTmp1 - fTmp0;
                            fDenom = fA00 - 2.0 * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fT = 1.0;
                                fS = 0.0;
                                fSqrDistance = fA11 + 2.0 * fB1 + fC;
                            }
                            else
                            {
                                fT = fNumer / fDenom;
                                fS = 1.0 - fT;
                                fSqrDistance = fS * (fA00 * fS + fA01 * fT + 2.0 * fB0) +
                                    fT * (fA01 * fS + fA11 * fT + 2.0 * fB1) + fC;
                            }
                        }
                        else
                        {
                            fT = 0.0;
                            if (fTmp1 <= 0.0)
                            {
                                fS = 1.0;
                                fSqrDistance = fA00 + 2.0 * fB0 + fC;
                            }
                            else if (fB0 >= 0.0)
                            {
                                fS = 0.0;
                                fSqrDistance = fC;
                            }
                            else
                            {
                                fS = -fB0 / fA00;
                                fSqrDistance = fB0 * fS + fC;
                            }
                        }
                    }
                    else  // region 1
                    {
                        fNumer = fA11 + fB1 - fA01 - fB0;
                        if (fNumer <= 0.0)
                        {
                            fS = 0.0;
                            fT = 1.0;
                            fSqrDistance = fA11 + 2.0 * fB1 + fC;
                        }
                        else
                        {
                            fDenom = fA00 - 2.0f * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fS = 1.0;
                                fT = 0.0;
                                fSqrDistance = fA00 + 2.0 * fB0 + fC;
                            }
                            else
                            {
                                fS = fNumer / fDenom;
                                fT = 1.0 - fS;
                                fSqrDistance = fS * (fA00 * fS + fA01 * fT + 2.0 * fB0) +
                                    fT * (fA01 * fS + fA11 * fT + 2.0 * fB1) + fC;
                            }
                        }
                    }
                }

                // account for numerical round-off error
                if (fSqrDistance < 0.0)
                {
                    fSqrDistance = 0.0;
                }

                //  m_kClosestPoint0 = m_rkVector;
                return (v0 + ((float)fS) * kEdge0 + ((float)fT) * kEdge1);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel.Expressions;

namespace Streamlines.ShapeFunction
{
    class BilinearIsoPara
    {
        
        //Properties___________________________________________________________________________________________________________________________________________________

        public Vector3d U1;
        public Vector3d U2;
        public Vector3d U3;
        public Vector3d U4;
        public Point3d P1;
        public Point3d P2;
        public Point3d P3;
        public Point3d P4;
        public double v;

        public int Direction;

        private double ξ;
        private double η;
        public Point3d NaturalCoordinate
        { 
            get { return new Point3d(ξ, η, 0.0); }
            set 
            { 
                ξ = value.X;
                η = value.Y;
            }
        }

        //Functions
        private double N1;
        private double N2;
        private double N3;
        private double N4;

        private double N1ξ;
        private double N1η;
        private double N2ξ;
        private double N2η;
        private double N3ξ;
        private double N3η;
        private double N4ξ;
        private double N4η;

        private double N1x;
        private double N1y;
        private double N2x;
        private double N2y;
        private double N3x;
        private double N3y;
        private double N4x;
        private double N4y;

        private double EpsilonX;
        private double EpsilonY;
        private double GammaXY;

        private double SigmaX;
        private double SigmaY;
        private double TauXY;

        private double Theta;

        //Jacobian Matrix
        private double Dxdξ;
        private double Dxdη;
        private double Dydξ;
        private double Dydη;
        private double DetJ;


        //Constructors___________________________________________________________________________________________________________________________________________________
        public BilinearIsoPara()
        {
            P1 = new Point3d();
            P2 = new Point3d();
            P3 = new Point3d();
            P4 = new Point3d();
            U1 = new Vector3d();
            U2 = new Vector3d();
            U3 = new Vector3d();
            U4 = new Vector3d();
            v = 0.0;
            Direction = 0;
        }

        public BilinearIsoPara(Point3d p1, Point3d p2, Point3d p3, Point3d p4, Vector3d u1, Vector3d u2, Vector3d u3, Vector3d u4, double poissons)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            U1 = u1;
            U2 = u2;
            U3 = u3;
            U4 = u4;
            v = poissons;

            Direction = 0;
        }

        //Methods___________________________________________________________________________________________________________________________________________________

        //sets direction
        public void ChangeDirection(int dir)
        {
            Direction = dir;
        }

        // Finds the natural coordinate representing the input coordinate by iteratively constricting the domain by halfing the domain each time in the natural coordinates 
        // and finding the resulting two domains in the cartesian coordinates and detecting which of the point the cartesian point is located in. 
        // The process is ended after 16 consecutive halfings and the centre of the final domain is used as the estimated location of the cartisian point in the natural coordinate system
        public Point3d CalculateNaturalCoordinate(Point3d cartesianPoint)
        {
            //assign the initial point locations
            Point3d carte1 = P1;
            Point3d carte2 = P2;
            Point3d carte3 = P3;
            Point3d carte4 = P4;
            Point3d natur1 = new Point3d(-1.0, 1.0, 0.0);
            Point3d natur2 = new Point3d(1.0, 1.0, 0.0);
            Point3d natur3 = new Point3d(-1.0, -1.0, 0.0);
            Point3d natur4 = new Point3d(1.0, -1.0, 0.0);

            Point3d carte5 = new Point3d();
            Point3d carte6 = new Point3d();
            Point3d natur5 = new Point3d();
            Point3d natur6 = new Point3d();

            PolylineCurve partA;

            int side = 0;

            //repeat the process of halfing the domain 16 times
            for (int i = 0; i < 16; i++)
            {
                //left and right halfing
                if(side == 0)
                {
                    //find mid points that divide the domain
                    natur5 = new Point3d((natur1.X + natur2.X) / 2, (natur1.Y + natur2.Y) / 2, 0.0);
                    natur6 = new Point3d((natur3.X + natur4.X) / 2, (natur3.Y + natur4.Y) / 2, 0.0);
                    carte5 = CartesianCoordinates(natur5);
                    carte6 = CartesianCoordinates(natur6);

                    //construct the closed polylines representing the left domain
                    partA = new PolylineCurve(new Point3d[5] { carte1, carte5, carte6, carte3, carte1 });

                    //find which part the point lies in and assign the values so that that part becomes the main part for the next iteration
                    int testContainment = (int)partA.Contains(cartesianPoint, new Plane(new Point3d(0.0, 0.0, 0.0), new Vector3d(0.0, 0.0, 1.0)), 0.00001);
                    if (testContainment == 1 || testContainment == 3)
                    {
                        natur2 = natur5;
                        natur4 = natur6;

                        carte2 = carte5;
                        carte4 = carte6;
                    }
                    else
                    {
                        natur1 = natur5;
                        natur3 = natur6;

                        carte1 = carte5;
                        carte3 = carte6;
                    }

                    //change which orientiation is halfed for next interation
                    side = 1;
                }
                //top and bottom halfing
                else
                {
                    //find mid points that divide the domain
                    natur5 = new Point3d((natur1.X + natur3.X) / 2, (natur1.Y + natur3.Y) / 2, 0.0);
                    natur6 = new Point3d((natur2.X + natur4.X) / 2, (natur2.Y + natur4.Y) / 2, 0.0);
                    carte5 = CartesianCoordinates(natur5);
                    carte6 = CartesianCoordinates(natur6);

                    //construct the closed polylines representing the top domain
                    partA = new PolylineCurve(new Point3d[5] { carte1, carte2, carte6, carte5, carte1 });

                    //find which part the point lies in and assign the values so that that part becomes the main part for the next iteration
                    int testContainment = (int)partA.Contains(cartesianPoint, new Plane(new Point3d(0.0, 0.0, 0.0), new Vector3d(0.0, 0.0, 1.0)), 0.00001);
                    if (testContainment == 1 || testContainment == 3)
                    {
                        natur3 = natur5;
                        natur4 = natur6;

                        carte3 = carte5;
                        carte4 = carte6;
                    }
                    else
                    {
                        natur1 = natur5;
                        natur2 = natur6;

                        carte1 = carte5;
                        carte2 = carte6;
                    }

                    //change which orientiation is halfed for next interation
                    side = 0;
                }
            }


            return (natur1+ natur2 + natur3 + natur4)/4;
        }

        //finds the point supplied in it's natural coordinate in the cartesian coordinate system by using the shape functions of the isoparametric element and the cartesian coordinate locations
        public Point3d CartesianCoordinates(Point3d naturalPoint)
        {
            //assign input values
            NaturalCoordinate = naturalPoint;

            //Calculate shape functions
            CalculateShapeFunctionValues();

            //find cartesian point
            double x = N1 * P1.X + N2 * P2.X + N3 * P3.X + N4 * P4.X;
            double y = N1 * P1.Y + N2 * P2.Y + N3 * P3.Y + N4 * P4.Y;
            return new Point3d(x, y, 0.0);
        }

        //calculates the values of the terms in the jacobian matrix, note that these are without the correct sign used in the matrix.
        private void CalculateJacobianTerms()
        {
            Dxdξ = N1ξ * P1.X + N2ξ * P2.X + N3ξ * P3.X + N4ξ * P4.X;
            Dxdη = N1η * P1.X + N2η * P2.X + N3η * P3.X + N4η * P4.X;
            Dydξ = N1ξ * P1.Y + N2ξ * P2.Y + N3ξ * P3.Y + N4ξ * P4.Y;
            Dydη = N1η * P1.Y + N2η * P2.Y + N3η * P3.Y + N4η * P4.Y;
        }

        //calculates the determinant of the jacobian matrix
        private void CalculateJacobianDeterminant()
        {
            DetJ = (Dydη * Dxdξ) - (Dydξ * Dxdη);
        }

        private void CalculateShapeFunctionValues()
        {
            //calculates the shape function values for the supplied natural coordinates for the isoparametric element
            N1 = (1.0 / 4) * (1 - ξ) * (1 + η);
            N2 = (1.0 / 4) * (1 + ξ) * (1 + η);
            N3 = (1.0 / 4) * (1 - ξ) * (1 - η);
            N4 = (1.0 / 4) * (1 + ξ) * (1 - η);
        }

        //calculates the values of the differentiated shape functions in the natural coordinate system
        private void CalculateDifferentiatedNaturalShapeFunctionValues()
        {
            //calculates the shape function values for the supplied natural coordinates for the isoparametric element
            N1ξ = -0.25 * η - 0.25;
            N1η = -0.25 * ξ + 0.25;
            N2ξ = +0.25 * η + 0.25;
            N2η = +0.25 * ξ + 0.25;
            N3ξ = +0.25 * η - 0.25;
            N3η = +0.25 * ξ - 0.25;
            N4ξ = -0.25 * η + 0.25;
            N4η = -0.25 * ξ - 0.25;
        }

        //calculates the values of differentiated shape functions in the cartesian coordinate system by using the differentiated shape functions from the natural coordiate system and the jacobian matrix equation
        private void CalculateDifferentiatedCartesianShapeFunctionValues()
        {
            CalculateDifferentiatedNaturalShapeFunctionValues();
            CalculateJacobianTerms();
            CalculateJacobianDeterminant();

            N1x = (1 / DetJ) * (Dydη * N1ξ - Dydξ * N1η);
            N1y = (1 / DetJ) * (- Dxdη * N1ξ + Dxdξ * N1η);

            N2x = (1 / DetJ) * (Dydη * N2ξ - Dydξ * N2η);
            N2y = (1 / DetJ) * (-Dxdη * N2ξ + Dxdξ * N2η);

            N3x = (1 / DetJ) * (Dydη * N3ξ - Dydξ * N3η);
            N3y = (1 / DetJ) * (-Dxdη * N3ξ + Dxdξ * N3η);

            N4x = (1 / DetJ) * (Dydη * N4ξ - Dydξ * N4η);
            N4y = (1 / DetJ) * (-Dxdη * N4ξ + Dxdξ * N4η);
        }

        
        public Vector3d Evaluate(Point3d location)
        {
            NaturalCoordinate = CalculateNaturalCoordinate(location);

            //calculate the shape function values
            CalculateDifferentiatedCartesianShapeFunctionValues();

            //calculate the strain values
            CalculateStrains();

            //calculate the stress values
            CalculateStresses();

            //Calculate theta
            CalculateTheta();

            //calculate the vector
            if (Direction == 1) return new Vector3d(Math.Cos(Theta), Math.Sin(Theta), 0);
            else if (Direction == 2) return new Vector3d(-Math.Sin(Theta), Math.Cos(Theta), 0);
            else return new Vector3d();
        }
        

        private void CalculateStrains()
        {
            EpsilonX = N1x * U1.X + N2x * U2.X + N3x * U3.X + N4x * U4.X;
            EpsilonY = N1y * U1.Y + N2y * U2.Y + N3y * U3.Y + N4y * U4.Y;
            GammaXY = N1y * U1.X + N2y * U2.X + N3y * U3.X + N4y * U4.X + N1x * U1.Y + N2x * U2.Y + N3x * U3.Y + N4x * U4.Y;
        }

        private void CalculateStresses()
        {
            //Creates a string for each stress function
            SigmaX = (EpsilonX + v * EpsilonY);
            SigmaY = (EpsilonY + v * EpsilonX);
            TauXY = (((1 - v) / 2) * GammaXY);
        }

        private void CalculateTheta()
        {
            Theta = Math.Atan(2*TauXY/(SigmaX - SigmaY))/2;

            if (SigmaY > (SigmaX + SigmaY) / 2)
            {
                if (TauXY > 0) Theta -= Math.PI / 2;
                else Theta += Math.PI / 2;
            } 
        }
        
    }
}

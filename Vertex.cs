using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Graph
{
    class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }

        public char Title { get; set; }
        public UIElement UIElement { get; set; }

        public List<Edge> Edges;

        public Vertex(double x, double y)
        {
            this.X = x;
            this.Y = y;
            Edges = new List<Edge>();
        }

        public Vertex(Point point) : this(point.X, point.Y)
        {
        }

        public int GetDistance(Vertex v2)
        {
            Edge edge = Edges.FirstOrDefault(e => (e.V1 == v2 || e.V2 == v2));
            if(edge == null)
            {
                throw new Exception("Ребра не существует");
            }
            return edge.Distance;
        }
    }
}

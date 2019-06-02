using System.Windows;

namespace Graph
{
    class Edge
    {
        public Vertex V1 { get; set; }
        public Vertex V2 { get; set; }
        public int Distance { get; set; }
        public UIElement UIElement { get; set; }
        public UIElement Line { get; set; }

        public Edge(Vertex v1, Vertex v2, int distance)
        {
            V1 = v1;
            V2 = v2;
            Distance = distance;
        }
    }
}

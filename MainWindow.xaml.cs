using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graph
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Vertex> vertexList;
        private List<Edge> edgeList;
        private char lastVertexTitle = 'A';

        public MainWindow()
        {
            InitializeComponent();
            vertexList = new List<Vertex>();
            edgeList = new List<Edge>();

            fromCB.ItemsSource = vertexList;
            toCB.ItemsSource = vertexList;
            startVertex.ItemsSource = vertexList;
            endVertex.ItemsSource = vertexList;
        }


        // Добавление вершины
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as Canvas);
            Vertex vertex = new Vertex(point.X, point.Y);
            vertex.Title = lastVertexTitle++;
            vertexList.Add(vertex);

            Grid nodeGrid = DrawVertex(vertex);
            vertex.UIElement = nodeGrid;
            mainCanvas.Children.Add(nodeGrid);

            fromCB.Items.Refresh();
            toCB.Items.Refresh();
            startVertex.Items.Refresh();
            endVertex.Items.Refresh();
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.Children.Clear();
            distanceTB.Text = "";
            vertexList.Clear();
            lastVertexTitle = 'A';
            fromCB.Items.Refresh();
            toCB.Items.Refresh();
            startVertex.Items.Refresh();
            endVertex.Items.Refresh();
        }

        private void RemoveVertex_Click(object sender, RoutedEventArgs e)
        {
            if (vertexList.Count == 0)
            {
                MessageBox.Show("Узлов нет");
                return;
            }
            lastVertexTitle--;
            mainCanvas.Children.Remove(vertexList.Last().UIElement);
            foreach (Edge edge in vertexList.Last().Edges)
            {
                mainCanvas.Children.Remove(edge.UIElement);
                mainCanvas.Children.Remove(edge.Line);
            }
            vertexList.Remove(vertexList.Last());
            fromCB.Items.Refresh();
            toCB.Items.Refresh();
            startVertex.Items.Refresh();
            endVertex.Items.Refresh();
        }

        private void SaveDistance_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(distanceTB.Text))
            {
                MessageBox.Show("Введите расстояние между вершинами");
                return;
            }
            int distance = Convert.ToInt32(distanceTB.Text);
            Vertex v1 = fromCB.SelectedItem as Vertex;
            Vertex v2 = toCB.SelectedItem as Vertex;

            if(v1 == null || v2 == null)
            {
                MessageBox.Show("Вершины указаны не верно");
                return;
            }

            if(v1 == v2)
            {
                MessageBox.Show("Нельзя указывать расстояние между одинаковыми вершинами");
                return;
            }

            Edge edge = new Edge(v1, v2, distance);
            edgeList.Add(edge);
            v1.Edges.Add(edge);
            v2.Edges.Add(edge);

            DrawEdge(edge);
        }

        private void FindShortestDistance_Click(object sender, RoutedEventArgs e)
        {
            Vertex v1 = startVertex.SelectedItem as Vertex;
            Vertex v2 = endVertex.SelectedItem as Vertex;

            #region Проверки
            if (v1 == null || v2 == null)
            {
                MessageBox.Show("Вершины указаны не верно");
                return;
            }
            if(v1 == v2)
            {
                MessageBox.Show("Расстояние между одной и той же вершиной равно нулю");
                return;
            }
            if(v1.Edges.Count == 0)
            {
                MessageBox.Show($"{v1.Title} не имеет смежных вершин!");
                return;
            }
            if (v2.Edges.Count == 0)
            {
                MessageBox.Show($"{v2.Title} не имеет смежных вершин!");
                return;
            }
            #endregion

            int[] distances = Enumerable.Repeat(int.MaxValue, vertexList.Count).ToArray();
            int[] temp_distances = new int[distances.Length];

            int startIndex = v1.Title - 'A';
            int endIndex = v2.Title - 'A';
            distances[startIndex] = 0;

            List<Vertex> sequence = new List<Vertex>();
            sequence.Add(vertexList[startIndex]);

            int lastIndex = startIndex;
            while (distances[endIndex] == int.MaxValue)
            {
                temp_distances = Enumerable.Repeat(int.MaxValue, vertexList.Count).ToArray();

                Vertex currentVertex = vertexList[lastIndex];
                IEnumerable<Vertex> neighbors = currentVertex.Edges.Select(edge => edge.V2 == currentVertex ? edge.V1 : edge.V2);

                //Находим близжайшего соседа
                foreach (Vertex neighbor in neighbors)
                {
                    if (distances[neighbor.Title - 'A'] != int.MaxValue) // если уже прошли этого соседа, то скипаем
                        continue;
                    int current_distance = neighbor.GetDistance(currentVertex);
                    if(temp_distances[neighbor.Title - 'A'] > current_distance)
                    {
                        temp_distances[neighbor.Title - 'A'] = current_distance;
                    }
                }

                int minValue = temp_distances.Min();
                lastIndex = Array.IndexOf<int>(temp_distances, minValue);
                distances[lastIndex] = minValue;
                sequence.Add(vertexList[lastIndex]);
            }
            int distanceResult = distances.Where(d => d != int.MaxValue).Sum(); 
            drawTree(sequence, distanceResult);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("тыры пыры схс");
        }

        // Результат в новом окне
        private void drawTree(List<Vertex> sequence, int distance)
        {
            TreeResult resultWindow = new TreeResult();
            foreach (Vertex vertex in sequence)
            {
                resultWindow.resultCanvas.Children.Add(DrawVertex(vertex));
            }
            for (int i = 1; i < sequence.Count; i++)
            {
                Line line = createLine(sequence[i - 1], sequence[i]);
                resultWindow.resultCanvas.Children.Add(line);
            }
            resultWindow.resultPrice.Text += $"из {sequence.First().Title} в {sequence.Last().Title}: {distance.ToString()}";

            resultWindow.Width = mainCanvas.ActualWidth;
            resultWindow.Height = mainCanvas.ActualHeight;
            resultWindow.ShowDialog();
        }


        // Вспомогательные функции
        private Grid DrawVertex(Vertex vertex)
        {
            Grid nodeGrid = createCircleWithText(30, 30, Color.FromRgb(255, 0, 0),
               vertex.Title.ToString(), 25, Color.FromRgb(255, 255, 255));

            nodeGrid.Margin = new Thickness(vertex.X, vertex.Y, 30, 30);

            Canvas.SetZIndex(nodeGrid, 2);
            return nodeGrid;
        }
        private Point GetLineCenter(Line line)
        {
            double x = (line.X1 + line.X2) / 2;
            double y = (line.Y1 + line.Y2) / 2;
            return new Point(x, y);
        }
        private void DrawEdge(Edge edge)
        {
            Line line = createLine(edge.V1, edge.V2);
            mainCanvas.Children.Add(line);

            Point lineCenter = GetLineCenter(line);

            Grid distance = createCircleWithText(20, 20, Color.FromRgb(0, 0, 0), 
                edge.Distance.ToString(), 15, Color.FromRgb(255, 255, 255));

            distance.Margin = new Thickness(lineCenter.X-10, lineCenter.Y-10, 0, 0);

            Canvas.SetZIndex(distance, 3);
            mainCanvas.Children.Add(distance);
            edge.UIElement = distance;
            edge.Line = line;
        }

        private Line createLine(Vertex v1, Vertex v2)
        {
            Line line = new Line();
            // +15 потому что нам нужны центры
            line.X1 = v1.X + 15;
            line.Y1 = v1.Y + 15;
            line.X2 = v2.X + 15;
            line.Y2 = v2.Y + 15;

            line.StrokeThickness = 3;
            line.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            Canvas.SetZIndex(line, 1);
            return line;
        }
        private Grid createCircleWithText(double ellipseWidth, double ellipseHeight, Color ellipseColor, string text, double fontSize,Color foreground)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Height = ellipseWidth;
            ellipse.Width = ellipseHeight;
            ellipse.Fill = new SolidColorBrush(ellipseColor);

            TextBlock ellipseText = new TextBlock();
            ellipseText.Text = text;
            ellipseText.Foreground = new SolidColorBrush(foreground);
            ellipseText.HorizontalAlignment = HorizontalAlignment.Center;
            ellipseText.VerticalAlignment = VerticalAlignment.Center;
            ellipseText.FontSize = fontSize;

            Grid grid = new Grid();
            grid.Children.Add(ellipse);
            grid.Children.Add(ellipseText);
            return grid;
        }

        
    }
}
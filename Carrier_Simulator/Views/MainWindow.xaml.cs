using Carrier_Simulator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Carrier_Simulator.MainWindow;
using static System.Collections.Specialized.BitVector32;
using Section = Carrier_Simulator.Models.Section;

namespace Carrier_Simulator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private Carrier _carrier;
        private Point lineStart = new Point(50, 150);           // 고정 시작점
        private Point lineEnd = new Point(870, 150);            // 고정 끝점
        private Line fixedLine;
        private double scale = 1.0; // mm per pixel
        private Rectangle carrierRect;
        public ObservableCollection<Section> Sections { get; } = new ObservableCollection<Section>();

        public MainWindow()
        {
            InitializeComponent();
            SectionGrid.ItemsSource = Sections;
            DarawFixedLine();
        }


        private void BtnSectionAdd_Click(object sender, RoutedEventArgs e)
        {
            //var nextNumber = Sections.Count + 1;
            Sections.Add(new Section { SectionNumber = Sections.Count + 1, Position = "" });
        }    

        private void BtnSetCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LengthBox.Text, out double length) &&
            double.TryParse(WeightBox.Text, out double weight) &&
            double.TryParse(SpeedBox.Text, out double speed) &&
            double.TryParse(AccelBox.Text, out double accel))
            {
                if (length <= 0 || weight <= 0 || speed <= 0)
                {
                    MessageBox.Show("길이, 무게, 속도는 0보다 커야 합니다.");
                    return;
                }

                _carrier = new Carrier
                {
                    Length = length,
                    Weight = weight,
                    Speed = speed,
                    Acceleration = accel
                };

                MessageBox.Show($"캐리어 설정 완료:\n" +
                                $"길이: {_carrier.Length}m\n" +
                                $"무게: {_carrier.Weight}kg\n" +
                                $"속도: {_carrier.Speed}m/s\n" +
                                $"가속도: {_carrier.Acceleration}m/s²");
            }
            else
            {
                MessageBox.Show("입력값을 정확히 입력해주세요.");
            }

            DrawCarrier(); // 캐리어 설정 후 캔버스에 그리기
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            Sections.Clear();
        }

        private void SectionGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // 예: 편집 중인 셀 정보 가져오기
            if (e.Column is DataGridTextColumn textColumn &&
                e.EditingElement is System.Windows.Controls.TextBox textBox &&
                e.Row.Item is Section section && TotalScale.Text.Length > 0)
            {
                double sectionLength = Convert.ToDouble(textBox.Text);
                string columnName = (textColumn.Binding as Binding)?.Path?.Path; //구간 번호

                if(sectionLength == 0)
                {
                    MessageBox.Show("구간 위치는 출발지점보다 커야 합니다.");
                    return;
                }
                else if(sectionLength >= Convert.ToDouble(TotalScale.Text))
                {
                    MessageBox.Show("구간 위치는 도착지점보다 작아야 합니다.");
                    return;
                }

                if (columnName == "Position")
                {
                    section.Position = sectionLength.ToString();
                    MessageBox.Show($"'{section.SectionNumber}'번 구간 위치가 '{sectionLength}'로 변경되었습니다.");
                    DrawMarkers();
                }
            }
        }

      

        #region Canvus

        private void DarawFixedLine()
        {
            double margin = 50; // 캔버스 여백

            double width = SimulationCanvas.ActualWidth;
            double height = SimulationCanvas.ActualHeight;

            if (width <= margin * 2 || height < 1) return; // 캔버스 크기가 너무 작으면 그리지 않음

            lineStart = new Point(margin, height / 2);              // 왼쪽 중간
            lineEnd = new Point(width - margin, height / 2);        // 오른쪽 중간

            SimulationCanvas.Children.Clear(); // 캔버스 초기화

            fixedLine = new Line
            {
                X1 = lineStart.X,
                Y1 = lineStart.Y,
                X2 = lineEnd.X,
                Y2 = lineEnd.Y,
                Stroke = Brushes.Blue,
                StrokeThickness = 3
            };

            SimulationCanvas.Children.Add(fixedLine);

            var startText = new TextBlock
            {
                Text = "0 mm",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Blue
            };
            SimulationCanvas.Children.Add(startText);
            Canvas.SetLeft(startText, lineStart.X - 20);
            Canvas.SetTop(startText, lineStart.Y + 25);

            // ✅ 시작점 표시선 (수직선)
            var startMark = new Line
            {
                X1 = lineStart.X,
                Y1 = lineStart.Y + 10,
                X2 = lineStart.X,
                Y2 = lineStart.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            SimulationCanvas.Children.Add(startMark);

            // ✅ 끝점 표시선 (수직선)
            var endMark = new Line
            {
                X1 = lineEnd.X,
                Y1 = lineEnd.Y + 10,
                X2 = lineEnd.X,
                Y2 = lineEnd.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            SimulationCanvas.Children.Add(endMark);
        }

        /// <summary>
        /// 숫자만 입력받도록 설정, 전체 스케일 입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            string input = textBox.Text;

            if (!double.TryParse(input, out double realLengthMm) && !string.IsNullOrWhiteSpace(input))
            {
                int caretIndex = textBox.CaretIndex - 1;
                textBox.Text = new string(input.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
                textBox.CaretIndex = Math.Max(0, caretIndex); // 커서 위치 복원
            }

            double pixelLength = Math.Sqrt(Math.Pow(lineEnd.X - lineStart.X, 2) + Math.Pow(lineEnd.Y - lineStart.Y, 2));
            //scale = realLengthMm / pixelLength; // mm per pixel 계산

            //ScaleInfoText.Text = $"계산된 스케일: 1px = {scale:F0} mm";

            DrawEndLabel(realLengthMm); // 이걸 따로 호출해야 합니다!
        }

        private void DrawEndLabel(double lengthMm)
        {
            // 기존 EndLabel만 지움 (Tag 사용)
            var existing = SimulationCanvas.Children.OfType<TextBlock>()
                .Where(tb => tb.Tag?.ToString() == "EndLabel").ToList();
            foreach (var tb in existing)
                SimulationCanvas.Children.Remove(tb);


            var endText = new TextBlock
            {
                Text = $"{lengthMm} mm",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Blue,
                Tag = "EndLabel"
            };
            SimulationCanvas.Children.Add(endText);
            Canvas.SetLeft(endText, lineEnd.X - 20);
            Canvas.SetTop(endText, lineEnd.Y + 25);
        }

        private void DrawMarkers()
        {
            if (fixedLine == null || scale <= 0)
            {
                MessageBox.Show("기준선과 스케일을 설정하세요");
                return;
            }

            var toRemove = SimulationCanvas.Children.OfType<UIElement>()
                .Where(el =>
                    (el is TextBlock tb && tb.Tag?.ToString() == "MarkerLabel") ||
                    (el is Line l && l.Stroke == Brushes.Green)
                ).ToList();

            toRemove.ForEach(el => SimulationCanvas.Children.Remove(el));

            double totalLengthMm;
            if (!double.TryParse(TotalScale.Text, out totalLengthMm) || totalLengthMm <= 0)
                return;

            foreach (var section in Sections)
            {
                double position = Convert.ToDouble(section.Position);
                double ratio = position / totalLengthMm; // 전체 길이에 대한 비율

                double x = lineStart.X + (lineEnd.X - lineStart.X) * ratio;
                double y = lineStart.Y + (lineEnd.Y - lineStart.Y) * ratio;

                // ✅ 표시선 (수직선)
                var markLine = new Line
                {
                    X1 = x,
                    Y1 = y,
                    X2 = x,
                    Y2 = y + 10,
                    Stroke = Brushes.Green,
                    StrokeThickness = 2
                };
                SimulationCanvas.Children.Add(markLine);

                // ✅ 거리 텍스트
                var label = new TextBlock
                {
                    Text = $"{position} mm",
                    FontWeight = FontWeights.Normal,
                    Foreground = Brushes.Black,
                    Tag = "MarkerLabel"
                };
                SimulationCanvas.Children.Add(label);
                Canvas.SetLeft(label, x - 20);
                Canvas.SetTop(label, y + 12);
            }
        }

        //private void RemoveMarker_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button btn && btn.DataContext is MarkerPoint marker)
        //        MarkerPoints.Remove(marker);
        //}

        private double GetLinePixelLength()
        {
            return Math.Sqrt(Math.Pow(lineEnd.X - lineStart.X, 2) + Math.Pow(lineEnd.Y - lineStart.Y, 2));
        }

        private void DrawCarrier()
        {
            if (_carrier == null)
            {
                MessageBox.Show("캐리어 속성을 설정하세요.");
                return;
            }

            if (!double.TryParse(TotalScale.Text, out double totalLengthMm) || totalLengthMm <= 0)
            {
                MessageBox.Show("전체 길이를 먼저 설정하세요.");
                return;
            }

            double ratio = _carrier.Length / totalLengthMm; // 전체 길이에 대한 비율
            double pixelWidth = (lineEnd.X - lineStart.X) * ratio; // 픽셀 단위 너비 계산
            
            if (carrierRect != null)
            {
                SimulationCanvas.Children.Remove(carrierRect); // 기존 캐리어 사각형 제거
            }
            carrierRect = new Rectangle
            {
                Width = pixelWidth,
                Height = 40,
                Fill = Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            double x = lineStart.X;
            double y = lineStart.Y - 50;

            Canvas.SetLeft(carrierRect, x);
            Canvas.SetTop(carrierRect, y);

            SimulationCanvas.Children.Add(carrierRect);
        }

        #endregion

        private void SimulationCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DarawFixedLine(); // 캔버스 크기 변경 시 고정 선 다시 그리기

            if (double.TryParse(TotalScale.Text, out double realLengthMm))
                DrawEndLabel(realLengthMm); // 캔버스 크기 변경 시 끝점 레이블 다시 그리기

            if (Sections.Count > 0) ;

            if (_carrier != null)
                DrawCarrier();
        }
    }
}

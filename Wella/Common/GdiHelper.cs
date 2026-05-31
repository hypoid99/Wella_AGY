using System.Drawing;
using System.Drawing.Drawing2D;

namespace Wella.Common
{
    public static class GdiHelper
    {
        /// <summary>
        /// 지정한 반경(Radius)을 갖는 둥근 사각형 경로(GraphicsPath)를 생성합니다.
        /// </summary>
        public static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // 좌측 상단 모서리
            path.AddArc(arc, 180, 90);

            // 우측 상단 모서리
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // 우측 하단 모서리
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // 좌측 하단 모서리
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// GDI+로 둥근 모서리 사각형을 채웁니다.
        /// </summary>
        public static void FillRoundedRectangle(Graphics g, Brush brush, Rectangle bounds, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRectanglePath(bounds, radius))
            {
                g.FillPath(brush, path);
            }
        }

        /// <summary>
        /// GDI+로 둥근 모서리 사각형의 테두리를 그립니다.
        /// </summary>
        public static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRectanglePath(bounds, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// 부드러운 그라데이션이 적용된 둥근 카드 배경을 렌더링합니다.
        /// </summary>
        public static void FillGradientRoundedRectangle(Graphics g, Rectangle bounds, Color colorStart, Color colorEnd, int radius, float angle = 90F)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRectanglePath(bounds, radius))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, colorStart, colorEnd, angle))
                {
                    g.FillPath(brush, path);
                }
            }
        }
    }
}

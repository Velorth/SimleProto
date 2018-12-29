using UnityEditor;
using UnityEngine;

namespace SimpleProtoEditor
{
    public static class LayoutMarkupExtension
    {
        public static Rect Top(this Rect rect)
        {
            return rect.Top(EditorGUIUtility.singleLineHeight);
        }

        public static Rect Top(this Rect rect, float height)
        {
            return new Rect(rect.xMin, rect.yMin, rect.width, height);
        }

        public static Rect Bottom(this Rect rect, float height)
        {
            return new Rect(rect.xMin, rect.yMin + rect.height - height, rect.width, height);
        }

        public static Rect Left(this Rect rect, float width)
        {
            return new Rect(rect.xMin, rect.yMin, width, rect.height);
        }

        public static Rect Right(this Rect rect, float width)
        {
            return new Rect(rect.xMax - width, rect.yMin, width, rect.height);
        }

        public static Rect NextHorizontal(this Rect rect, float width)
        {
            return NextHorizontal(rect, width, 2);
        }

        public static Rect NextHorizontal(this Rect rect, float width, float spacing)
        {
            return new Rect(rect.xMax + spacing, rect.yMin, width, rect.height);
        }

        public static Rect NextVertical(this Rect rect)
        {
            return rect.NextVertical(EditorGUIUtility.singleLineHeight);
        }

        public static Rect NextVertical(this Rect rect, float height)
        {
            return rect.NextVertical(height, EditorGUIUtility.standardVerticalSpacing);
        }

        public static Rect NextVertical(this Rect rect, float height, float spacing)
        {
            return new Rect(rect.xMin, rect.yMax + spacing, rect.width, height);
        }

        public static Rect Margin(this Rect rect, float left, float right, float top, float bottom)
        {
            return new Rect(rect.xMin + left, rect.yMin + top, rect.width - left - right, rect.height - top - bottom);
        }
    }
}

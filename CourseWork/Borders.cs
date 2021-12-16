using System;
using System.Collections.Generic;
using System.Text;

namespace CourseWork
{
    class Borders
    {

        private List<float> left;
        private List<float> right;

        public Borders(List<float> left, List<float> right) {
            this.left = left;
            this.right = right;
        }

        public List<float> getLeft() {
            return left;
        }

        public List<float> getRight()
        {
            return right;
        }
    }
}

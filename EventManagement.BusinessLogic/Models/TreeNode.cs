using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class TreeNode<T>
    {
        public List<TreeNode<T>> Children = new List<TreeNode<T>>();

        public T Item { get; set; }

        public TreeNode(T item)
        {
            Item = item;
        }

        public TreeNode<T> AddChild(T item)
        {
            TreeNode<T> child = new TreeNode<T>(item);
            Children.Add(child);
            return child;
        }
    }
}

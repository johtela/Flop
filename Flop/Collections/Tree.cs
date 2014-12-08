namespace Flop.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Flop.Visuals;

    /// <summary>
    /// Abstract tree data structure.
    /// </summary>
    /// <typeparam name="K">The key type of the tree.</typeparam>
    public abstract class Tree<K> : IVisualizable where K : IComparable<K>
    {
        /// <summary>
        /// Returns the left subtree of this tree.
        /// </summary>
        protected internal abstract Tree<K> Left { get; }

        /// <summary>
        /// Returns the right subtree of this tree.
        /// </summary>
        protected internal abstract Tree<K> Right { get; }

        /// <summary>
        /// Returns the key of this tree.
        /// </summary>
        protected internal abstract K Key { get; }

        /// <summary>
        /// Returns the weight of the tree. That is, the number of nodes in the tree including
        /// the root.
        /// </summary>
        protected internal abstract int Weight { get; }

        /// <summary>
        /// Clones the root of the tree.
        /// </summary>
        /// <param name="newLeft">The tree that is assigned to left subtree of the cloned root.</param>
        /// <param name="newRight">The tree that is assigned to right subtree of the cloned root.</param>
        /// <param name="inPlace">If the inPlace parameter is true, the root is mutated in place, 
        /// instead of copying it.</param>
        /// <returns>The cloned root tree.</returns>
        protected internal abstract Tree<K> Clone (Tree<K> newLeft, Tree<K> newRight, bool inPlace);

        /// <summary>
        /// Tests if the tree is empty.
        /// </summary>
        /// <returns>True, if the tree is an empty tree; false otherwise.</returns>
        protected internal abstract bool IsEmpty ();

        #region IVisualizable implementation

        private Visual NodeVisual (string text, Visual parent)
        {
            var node = Visual.Frame (Visual.Margin (Visual.Label (text), 2, 2, 2, 2), FrameKind.Ellipse);
            return Visual.Anchor (
                parent == null ? node : Visual.Connector (node, parent, HAlign.Center, VAlign.Top),
                HAlign.Center, VAlign.Bottom);
        }

        private Visual TreeVisual (Visual parent)
        {
            if (IsEmpty ())
                return Visual.Margin (NodeVisual ("-", parent), right: 4, bottom: 4);
            var node = NodeVisual (Key.ToString () + ":" + Weight.ToString (), parent);
            return Visual.VStack (HAlign.Center, Visual.Margin (node, right: 4, bottom: 20),
                Visual.HStack (VAlign.Top, Left.TreeVisual (node), Right.TreeVisual (node)));
        }

        public Visual ToVisual ()
        {
            return TreeVisual (null);
        }

        #endregion
    }

    /// <summary>
    /// Static class that contains the operations for trees.
    /// </summary>
    /// <typeparam name="T">The tree type.</typeparam>
    /// <typeparam name="K">The key type of the tree.</typeparam>
    public static class Tree<T, K>
        where T : Tree<K>
        where K : IComparable<K>
    {
        /// <summary>
        /// The comparer class for comparing trees.
        /// </summary>
        private class Comparer : IComparer<T>
        {
            /// <summary>
            /// Returns the comparison between two tree nodes.
            /// </summary>
            /// <param name="x">The first tree.</param>
            /// <param name="y">The second tree.</param>
            /// <returns>-1, if the key of tree x is less than key of tree y.<br/>
            /// 0, if the key of tree x is less than key of tree y.<br/>
            /// 1, oif the key of tree x is greater that key of tree y.</returns>
            public int Compare (T x, T y)
            {
                return x.Key.CompareTo (y.Key);
            }
        }

        private static Comparer _comparer = new Comparer ();
        internal static T _empty;

        /// <summary>
        /// Helper method for getting the left subtree of a tree.
        /// </summary>
        /// <param name="tree">The tree whose subtree is returned.</param>
        /// <returns>The left subtree of the tree.</returns>
        private static T Left (Tree<K> tree)
        {
            return (T)tree.Left;
        }

        /// <summary>
        /// Helper method for getting the right subtree of a tree.
        /// </summary>
        /// <param name="tree">The tree whose subtree is returned.</param>
        /// <returns>The right subtree of the tree.</returns>
        private static T Right (Tree<K> tree)
        {
            return (T)tree.Right;
        }

        /// <summary>
        /// Search for a given key in the tree.
        /// </summary>
        /// <param name="tree">The tree from which the key is searched.</param>
        /// <param name="key">The key that is searched.</param>
        /// <returns>The subtree that contains the given key, or an empty tree
        /// if the key is not found.</returns>
        public static T Search (T tree, K key)
        {
            while (true)
            {
                if (tree.IsEmpty ())
                    return tree;

                int compare = key.CompareTo (tree.Key);

                if (compare == 0)
                    return tree;
                else if (compare > 0)
                    tree = Right (tree);
                else
                    tree = Left (tree);
            }
        }

        /// <summary>
        /// Add a new item to the tree.
        /// </summary>
        /// <param name="tree">The tree to which the new item is added.</param>
        /// <param name="item">The item to be added.</param>
        /// <param name="height">The height of the node specified by the 
        /// tree<see cref="tree"/> parameter.</param>
        /// <returns>A new tree that contains the given item.</returns>
        private static T Add (T tree, T item, ref int height)
        {
            if (tree.IsEmpty ())
                return item;

            height++;
            return item.Key.CompareTo (tree.Key) > 0 ?
                (T)tree.Clone (tree.Left, Add (Right (tree), item, ref height), false) :
                (T)tree.Clone (Add (Left (tree), item, ref height), tree.Right, false);
        }

        public static T Add (T tree, T item)
        {
            var height = 0;
            T result = Add (tree, item, ref height);

            if (height > (10 * Math.Log (tree.Weight + 1, 2)))
                result = Rebalance (result);
            return result;
        }

        /// <summary>
        /// Remove the item with a given key from the tree.
        /// </summary>
        /// <param name="tree">The tree from where the item is removed.</param>
        /// <param name="key">The key of the item to be removed.</param>
        /// <returns>A new tree from which the item with the given key is removed.</returns>
        public static T Remove (T tree, K key)
        {
            if (tree.IsEmpty ())
                return tree;

            int compare = key.CompareTo (tree.Key);

            if (compare == 0)
            {
                // We have a match. If this is a leaf, just remove it 
                // by returning Empty.  If we have only one child,
                // replace the node with the child.
                if (Right (tree).IsEmpty () && Left (tree).IsEmpty ())
                    return Right (tree);
                else if (Right (tree).IsEmpty () && !Left (tree).IsEmpty ())
                    return Left (tree);
                else if (!Right (tree).IsEmpty () && Left (tree).IsEmpty ())
                    return Right (tree);
                else
                {
                    // We have two children. Remove the next-highest node and replace
                    // this node with it.
                    T successor = Right (tree);
                    while (!Left (successor).IsEmpty ())
                        successor = Left (successor);
                    return (T)successor.Clone (tree.Left, Remove (Right (tree), successor.Key), false);
                }
            }
            else if (compare < 0)
                return (T)tree.Clone (Remove (Left (tree), key), Right (tree), false);
            else
                return (T)tree.Clone (Left (tree), Remove (Right (tree), key), false);
        }

        /// <summary>
        /// Replace an item with another one with the same key.
        /// </summary>
        /// <param name="tree">The tree where the item is replaced.</param>
        /// <param name="key">The key to be searched for.</param>
        /// <param name="item">The item to be replaced.</param>
        /// <returns>A new tree that contains the item with given key.</returns>
        public static T Replace (T tree, K key, T item)
        {
            if (!key.Equals (item.Key))
                throw new ArgumentException ("Key must be the same in the item to be replaced");
            if (tree.IsEmpty ())
                throw new ArgumentException (string.Format ("Key '{0}' not found in the tree", key));

            var comp = key.CompareTo (tree.Key);
            if (comp == 0)
                return (T)item.Clone (tree.Left, tree.Right, false);
            else if (comp > 0)
                return (T)tree.Clone (tree.Left, Replace (Right (tree), key, item), false);
            else
                return (T)tree.Clone (Replace (Left (tree), key, item), tree.Right, false);
        }

        /// <summary>
        /// Traverse the tree depth first and in-order.
        /// </summary>
        /// <param name="tree">The tree to be traversed.</param>
        /// <returns>An enumerator that provides the items of the tree in the correct order.
        /// </returns>
        public static IEnumerable<T> TraverseDepthFirst (T tree)
        {
            var stack = new Stack<T> ();

            for (T current = tree; !current.IsEmpty () || stack.Count > 0; current = Right (current))
            {
                while (!current.IsEmpty ())
                {
                    stack.Push (current);
                    current = Left (current);
                }
                current = stack.Pop ();
                yield return current;
            }
        }

        public static IEnumerable<Tuple<T, int>> TraverseBreadthFirst (T tree)
        {
            if (tree.IsEmpty ()) yield break;
            var queue = new Queue<Tuple<T, int>> ();
            queue.Enqueue (Tuple.Create (tree, 0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue ();
                var left = Left (current.Item1);
                var right = Right (current.Item1);
                if (!left.IsEmpty ())
                    queue.Enqueue (Tuple.Create (left, current.Item2 + 1));
                if (!right.IsEmpty ())
                    queue.Enqueue (Tuple.Create (right, current.Item2 + 1));
                yield return current;
            }
        }

        public static void Iterate (T tree, Action<int, T> action, int i)
        {
            if (!tree.IsEmpty ())
            {
                if (tree.Weight >= 1000)
                {
                    Parallel.Invoke (
                        () => Iterate (Left (tree), action, i),
                        () => Iterate (Right (tree), action, i + Left (tree).Weight + 1));
                    action (i + Left (tree).Weight, tree);
                }
                else
                {
                    Iterate (Left (tree), action, i);
                    i += Left (tree).Weight;
                    action (i++, tree);
                    Iterate (Right (tree), action, i);
                }
            }
        }

        /// <summary>
        /// Return the items of the tree in an array.
        /// </summary>
        /// <param name="tree">The tree to be traversed.</param>
        /// <returns>An array that contains the items of the the tree in-order.</returns>
        public static T[] ToArray (T tree)
        {
            var result = new T[tree.Weight];

            Iterate (tree, (i, t) => result[i] = t, 0);
            return result;
        }

        /// <summary>
        /// Inserts the items in the array to a new tree.
        /// </summary>
        /// <param name="array">The array that contains the items. The array is sorted in-place
        /// and the items are attached to the tree. This means that the callers should not use
        /// the list any more after this function returns.</param>
        /// <returns>A new tree that contains the items in the list.</returns>
        public static T FromArray (T[] array, bool throwIfDuplicate)
        {
            if (array.Length == 0)
                return _empty;

            Array.Sort (array, _comparer);
            var last = RemoveDuplicates (array, throwIfDuplicate);
            return RebalanceList (array, 0, last, true);
        }

        public static int RemoveDuplicates (T[] array, bool throwIfDuplicate)
        {
            var res = 0;

            for (int i = 1; i < array.Length; i++)
            {
                if (array[res].Key.CompareTo (array[i].Key) != 0)
                    array[++res] = array[i];
                else if (throwIfDuplicate)
                    throw new ArgumentException ("Duplicate key: " + array[i].Key);
            }
            return res;
        }

        /// <summary>
        /// Rebalances the items in the tree.
        /// </summary>
        /// <param name="tree">The tree to be rebalanced.</param>
        /// <returns>A new tree that contains the same items as the original tree,
        /// but is in perfect balance.</returns>
        private static T Rebalance (T tree)
        {
            var array = ToArray (tree);
            return RebalanceList (array, 0, array.Length - 1, false);
        }

        /// <summary>
        /// Rebalances the items in a list.
        /// </summary>
        /// <param name="array">The array that contains the items to be rebalanced.</param>
        /// <param name="low">The index of the lowest item in the list.</param>
        /// <param name="high">The index of the highest item in the list.</param>
        /// <param name="inPlace">If the inPlace parameter is true, then the items
        /// in the list are recycled in the new tree. That is, the items are mutated
        /// to create the new tree.</param>
        /// <returns></returns>
        private static T RebalanceList (T[] array, int low, int high, bool inPlace)
        {
            var len = high - low + 1;
            if (len > 0)
            {
                var middle = (low + high) / 2;
                if (len >= 1000)
                {
                    var rebLeft = Task.Run (() => RebalanceList (array, low, middle - 1, inPlace));
                    var rebRight = Task.Run (() => RebalanceList (array, middle + 1, high, inPlace));
                    Task.WaitAll (rebLeft, rebRight);
                    return (T)array[middle].Clone (rebLeft.Result, rebRight.Result, inPlace);
                }
                else
                    return (T)array[middle].Clone (
                        RebalanceList (array, low, middle - 1, inPlace),
                        RebalanceList (array, middle + 1, high, inPlace), inPlace);
            }
            else
                return _empty;
        }
    }
}
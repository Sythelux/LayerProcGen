using System.Collections.Generic;
using System.Linq;

namespace Godot;

// from here: https://github.com/godotengine/godot/pull/66506
///<author email="a.schaub@lefx.de">Aron Schaub</author>
public static class NodeExtensions
{
    /// <summary>
    /// Returns a child node by its Type. Optionally pass a bool to make it look in children of children etc.
    /// To access a child node via its name, use <see cref="GetNode"/>.
    /// To access a child node via its name, use <see cref="GetChild{T}(int,bool)"/>.
    /// </summary>
    /// <seealso cref="GetChildOrNull{T}(int, bool)"/>
    /// <param name="recursive">If not set will only look through immediate children of this node.
    /// If set it will go all the way to the leaves of the tree.
    /// </param>
    /// <typeparam name="T">The type to cast to. Should be a descendant of <see cref="Node"/>.</typeparam>
    /// <returns>
    /// The first child <see cref="Node"/> of that type to be found or default (probably null) if no child was found./>.
    /// </returns>
    public static T GetChild<T>(this Node parent, bool recursive = false) where T : Node
    {
        return parent.GetChildren<T>(recursive).FirstOrDefault();
    }

    /// <summary>
    /// Iterates through all children of this Node and returns all which match the type.
    /// It will use Level-Order-Search which is a breadth first approach. The idea is, that the element,
    /// which you are trying to search for, is an immediate or at most a grandchild of the Node.
    /// </summary>
    /// <param name="recursive">If not set will only look through immediate children of this node.
    /// If set it will go all the way to the leaves of the tree.
    /// </param>
    /// <typeparam name="T">Type which all found children have</typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetChildren<T>(this Node parent, bool recursive = false) where T : Node
    {
        if (recursive)
        {
            var levelOrderSearch = new LevelOrderSearch(parent);
            foreach (var node in levelOrderSearch.Iterate<T>())
            {
                yield return node;
            }
            /*foreach (var child in GetChildren()) // (Depth first approach)
            {
                foreach (var node in child.GetChildren<T>())
                {
                    yield return node;
                }
            }*/
        }
        else
        {
            foreach (var node in parent.GetChildren().OfType<T>())
            {
                yield return node;
            }
        }
    }
}
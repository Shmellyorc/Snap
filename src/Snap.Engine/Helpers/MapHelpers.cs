namespace Snap.Engine.Helpers;

/// <summary>
/// Provides helper methods for converting between tile-based map coordinates
/// and world-space positions.
/// </summary>
/// <remarks>
/// This static utility class includes methods for:
/// <list type="bullet">
///   <item>
///     <description>Mapping grid locations to world-space coordinates (<see cref="MapToWorld"/>).</description>
///   </item>
///   <item>
///     <description>Mapping world-space positions back to grid coordinates (<see cref="WorldToMap"/>).</description>
///   </item>
///   <item>
///     <description>Converting between 1D tile indices and 2D coordinates (<see cref="To2D"/> and <see cref="To1D"/>).</description>
///   </item>
/// </list>
/// These conversions are useful in tile-based games or applications where
/// positions need to be translated between logical grid space and pixel space.
/// </remarks>
public static class MapHelpers
{
	/// <summary>
	/// Converts a tile-based grid location into world-space coordinates.
	/// </summary>
	/// <param name="location">The grid location.</param>
	/// <param name="tilesize">The size of one tile.</param>
	/// <returns>World-space coordinates in pixels.</returns>
	public static Vect2 MapToWorld(in Vect2 location, int tilesize)
		=> Vect2.Floor(location * tilesize);

	/// <summary>
	/// Converts a world-space position into map grid coordinates.
	/// </summary>
	/// <param name="position">World-space position.</param>
	/// <param name="tilesize">The size of one tile.</param>
	/// <returns>Tile-based grid coordinates.</returns>
	public static Vect2 WorldToMap(in Vect2 position, int tilesize)
		=> Vect2.Floor(position / tilesize);

	/// <summary>
	/// Converts a 1‑dimensional tile index into a 2D coordinate.
	/// </summary>
	/// <param name="index">The flat index.</param>
	/// <param name="tilesize">The width (and height) of the tile grid.</param>
	/// <returns>A <see cref="Vect2"/> representing the (x, y) tile position.</returns>
	public static Vect2 To2D(int index, int tilesize) =>
		new(index % tilesize, index / tilesize);

	/// <summary>
	/// Converts a 2D tile coordinate into a 1‑dimensional index.
	/// </summary>
	/// <param name="location">The (x, y) tile position.</param>
	/// <param name="tilesize">The width (and height) of the tile grid.</param>
	/// <returns>The flat index corresponding to <paramref name="location"/>.</returns>
	public static int To1D(Vect2 location, int tilesize) =>
		(int)location.Y * tilesize + (int)location.X;



	public static List<Vect2> ToMap(Vect2 size, Vect2 location, int tileSize)
	{
		var xSize = (int)MathF.Floor(size.X / tileSize);
		var ySize = (int)MathF.Floor(size.Y / tileSize);
		var result = new List<Vect2>(xSize * ySize); // 20 => 21 => 21

		for (int y = 0; y < ySize; y++)
		{
			for (int x = 0; x < xSize; x++)
				result.Add(location + new Vect2(x, y));
		}

		return result;
	}



	/// <summary>
	/// Determines whether a unit located at <paramref name="bLocation"/>is adjacent to <paramref name="aLocation"/> on a tile grid.
	/// </summary>
	/// <remarks>
	/// <para>This method operates on tile coordinates, not pixel coordinates.</para>
	/// <list type="bullet">
	///   <item>
	///     <description>
	///       If <paramref name="includeCorners"/> is <c>false</c>, adjacency is restricted to the 
	///       four cardinal directions (up, down, left, right). In this case, only tiles at a 
	///       distance of 1 are considered adjacent.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       If <paramref name="includeCorners"/> is <c>true</c>, diagonal tiles are also considered 
	///       adjacent. In this case, tiles up to a distance of 2 are considered potentially adjacent.
	///     </description>
	///   </item>
	/// </list>
	/// <para>
	/// The distance check serves as a quick guard clause to rule out locations that are too far away 
	/// to be adjacent before scanning the neighbor list.
	/// </para>
	/// </remarks>
	/// <param name="aLocation">The tile location of the reference unit (parent).</param>
	/// <param name="bLocation">The tile location of the unit being checked (child).</param>
	/// <param name="includeCorners">
	/// If <c>true</c>, diagonal tiles are considered adjacent in addition to orthogonal tiles.  
	/// If <c>false</c>, only orthogonal tiles are considered.
	/// </param>
	/// <returns>
	/// <c>true</c> if <paramref name="bLocation"/> is the same tile as <paramref name="aLocation"/> 
	/// or is an adjacent tile (depending on <paramref name="includeCorners"/>); otherwise, <c>false</c>.
	/// </returns>
	public static bool IsUnitAround(Vect2 aLocation, Vect2 bLocation, bool includeCorners)
	{
		if (aLocation == bLocation)
			return true;

		if (includeCorners)
		{
			if (aLocation.Distance(bLocation) > 2)
				return false;
		}
		else
		{
			if (aLocation.Distance(bLocation) > 1)
				return false;
		}

		Vect2[] neighbours = includeCorners
			?
				[
					aLocation + Vect2.Up,
					aLocation + Vect2.Right,
					aLocation + Vect2.Down,
					aLocation + Vect2.Left,

					aLocation + new Vect2(-1),		// Top Left
					aLocation + new Vect2(1, -1),	// Top Right
					aLocation + new Vect2(-1, 1),	// Bottom Left
					aLocation + new Vect2(1),		// Bottom Right
				]
			:
				[
					aLocation + Vect2.Up,
					aLocation + Vect2.Right,
					aLocation + Vect2.Down,
					aLocation + Vect2.Left,
				];

		for (int i = neighbours.Length - 1; i >= 0; i--)
		{
			var neighbour = neighbours[i];

			if (bLocation != neighbour)
				continue;

			return true;
		}

		return false;
	}
}

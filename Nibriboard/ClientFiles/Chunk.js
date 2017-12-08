"use strict";

import { point_line_distance_multi } from 'line-distance-calculator';

/**
 * Represents a single chunk on a plane.
 * Note that this is the client's representation of the chunk, so it's likely
 * to be a little different to the server's representation.
 */
class Chunk
{
	/**
	 * Creates a new chunk.
	 * @param 	{ChunkReference}	inChunkRef	The location of the new chunk.
	 * @param	{number}			inSize		The size of this chunk.
	 */
	constructor(inChunkRef, inSize)
	{
		/** @type {ChunkReference} */
		this.chunkRef = inChunkRef;
		/** @type {number} */
		this.size = inSize;
		/** @type {[ { Points: [Vector], Width: number, Color: string }] */
		this.lines = [];
	}
	
	/**
	 * Fetches a line in this chunk by it's unique id.
	 * @param  {string} uniqueLineId The target unique id to search for.
	 * @return {object|null}   The requested line, or null if it wasn't found.
	 */
	getLineByUniqueId(uniqueLineId)
	{
		for (let line of this.lines) {
			if(line.UniqueId == uniqueLineId)
				return line;
		}
		return null;
	}
	
	/**
	 * Whether this chunk is located at the specified chunk reference.
	 * @param	{ChunkReference}	otherChunkRef	The chunk reference to check
	 *                                       		ourselves against.
	 * @return	{bool}				Whether this chunk is located at the
	 *                           	specified chunk reference.
	 */
	isAt(otherChunkRef)
	{
		if(this.chunkRef.toString() == otherChunkPos.toString())
			return true;
		return false;
	}
	
	/**
	 * Whether this chunk falls inside the specified rectangle.
	 * @param  {Rectangle}	area	The rectangle to test against, in location-space
	 * @return {Boolean}	Whether this chunk falls inside the specified rectangle.
	 */
	isVisible(area)
	{
		let chunkArea = new Rectangle(
			this.chunkRef.x * this.size,
			this.chunkRef.y * this.size,
			this.size, this.size
		);
		return area.overlaps(area);
	}
	
	/**
	 * Fetches the last line segment that lies underneath the specified point.
	 * Prefers lines drawn later to lines drawn earlier.
	 * @param  {Vector} point The point to find the line underneath.
	 * @return {object|null}       The first line (segment) that lies underneath the specified point, or null if one couldn't be found.
	 */
	getLineUnderPoint(point)
	{
		// Prefer lines that have been drawn later (i.e. on top)
		for (let i = this.lines.length - 1; i > 0; i--) {
			// If our distance to the line is less than half the width (i.e. 
			// the radius), then we must be inside it
			if(point_line_distance_multi(point, this.lines[i].Points) <= this.lines[i].Width / 2)
				return this.lines[i];
		}
	}
	
	update(dt)
	{
		
	}
	
	/**
	 * Renders this chunk to the given canvas with the given context.
	 * @param	{HTMLCanvasElement}			canvas	The canvas to render to.
	 * @param	{CanvasRenderingContext2D}	context	The context to render with.
	 * @param	{ChunkCache}	chunkCache	The chunk cache to use to fetch data from surrounding chunks.
	 * @param	{Rectangle}		chunkArea	The area in which chunks are being rendered.
	 */
	render(canvas, context, chunkCache, chunkArea)
	{
		var planeSpaceRef = this.chunkRef.inPlaneSpace(this.size);
		
		context.save();
		context.translate(planeSpaceRef.x, planeSpaceRef.y);
		
		for(let line of this.lines)
		{
			// Don't draw lines that are walked by other chunks
			if(line.ContinuesFrom != null &&
				!(chunkCache.fetchChunk(line.ContinuesFrom) instanceof Chunk))
				continue;
			
			let linePoints = line.Points;
			
			// Fetch all the points on fragments of this line forwards from here
			if(line.ContinuesIn != null) {
				let nextLines = chunkCache.fetchLineFragments(line.ContainingChunk, line.UniqueId);
				linePoints = [];
				for (let nextLine of nextLines) {
					linePoints = linePoints.concat(nextLine.Points);
				}
			}
			
			context.beginPath();
			context.moveTo(
				linePoints[0].x - planeSpaceRef.x,
				linePoints[0].y - planeSpaceRef.y
			);
			
			for(let i = 1; i < linePoints.length; i++)
			{
				context.lineTo(
					linePoints[i].x - planeSpaceRef.x,
					linePoints[i].y - planeSpaceRef.y
				);
			}
			
			context.lineCap = "round";
			context.lineJoin = "round";
			context.lineWidth = line.Width;
			context.strokeStyle = line.Colour;
			context.stroke();
		}
		
		context.restore();
	}
}

export default Chunk;

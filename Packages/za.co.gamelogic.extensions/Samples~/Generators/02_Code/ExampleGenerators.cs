using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;

namespace Gamelogic.Extensions.Samples
{
	/// <exclude />
	// This class shows more examples of using generators
	public static class ExampleGenerators
	{
		//Complicated generators are best implemented using a custom class
		//This is an example of the class that implements a generator
		//that will generate the paper folding sequence.
		// You can use this generator like this:
		//
		// var generator = new PaperFoldingGenerator();
		// ...
		// var val = generator.Next();
		private sealed class PaperFoldingGenerator : Generator.AbstractGenerator<int>
		{
			private readonly IGenerator<int> count;
			private readonly Queue<int> queue;
			private int current;

			public override int Current => current;

			public PaperFoldingGenerator()
			{
				count = Generator.Count(4);
				queue = new Queue<int>();
			
				MoveNext();
			}

			public override void MoveNext()
			{
				var actionIndex = count.Next();

				switch (actionIndex)
				{
					case 0:
						current = 1;
						break;
					case 1:
						current = queue.Dequeue();
						break;
					case 2:
						current = 0;
						break;
					case 3:
						current = queue.Dequeue();
						break;
				}

				queue.Enqueue(current);
			}

			public override IGenerator<int> CloneAndRestart() => new PaperFoldingGenerator();
		}
		
		public static IGenerator<int> PaperFoldingSequence() => new PaperFoldingGenerator();

		//Returns a generator that generates points around a circle
		public static IGenerator<Vector2> Circle(float radius, int sampleCount)
		{
			var xGenerator = Generator
				.OpenSawTooth(sampleCount)
				.Select(x => radius * Mathf.Cos(2*Mathf.PI*x));

			var yGenerator = Generator
				.OpenSawTooth(sampleCount)
				.Select(x => radius * Mathf.Sin(2 * Mathf.PI * x));

			return Generator.Combine(xGenerator, yGenerator, (x, y) => new Vector2(x, y));
		}

		//Returns a generator that generates "box-blurred" floats of
		//the elements generated by the source generator.
		public static IGenerator<float> BoxBlur(IGenerator<float> source, int sampleCount) 
			=> source
				.Window(sampleCount)
				.Select(window => window.Average());

		//Returns a generator that generates the derivative of elements generated by the source
		public static IGenerator<float> Differentiate(this IGenerator<float> source) 
			=> source
				.Window(2)
				.Select(window => window[1] - window[0]);

		//Returns a generator that generates the nth derivative of elements generated by the source
		public static IGenerator<float> Differentiate(this IGenerator<float> generator, int n)
		{
			while (true)
			{
				switch (n)
				{
					case 0:
						return generator.CloneAndRestart();
					case 1:
						return generator.Differentiate();
					default:
						generator = generator.Differentiate();
						n -= 1;
						break;
				}
			}
		}

		//Generates a binary pattern 
		public static IGenerator<int> Pattern()
		{
			var zero = Generator.Constant(0);
			var one = Generator.Constant(1);
			var choice = Generator.Count(2);

			var alternate1 = Generator.Choose(new List<IGenerator<int>>{zero, one}, choice);
			var alternate2 = alternate1.RepeatEach(2);

			//will generate 0 0 0 1 1 0 1 1 ...//repeat
			var bin4 = Generator.Choose(new List<IGenerator<int>> {alternate1, alternate2}, choice);

			return bin4;
		}

		//Generates a sequence that will not have consecutive values repeat,
		//or have consecutive differences equal 
		//For example, this generator cannot generate 0 0, or 0 1 2
		public static IGenerator<int> ReallyRandom(int n)
		{
			return Generator.UniformRandomInt(n)
				.Window(2)
				.Where(w => w[0] != w[1])
				.Select(w => w[0])
				.Window(3)
				.Where(w => w[2] - w[1] != w[1] - w[0])
				.Select(w => w[0]);
		}

		//Generates the Fibonacci numbers
		public static IGenerator<int> Fibonacci() 
			=> Generator.Iterate(1, 1, (x, y) => x + y);

		//Generates the convolution of the elements of the source 
		//generator with a list of values.
		public static IGenerator<float> Convolution(this IGenerator<float> source, IEnumerable<float> convolutionWindow)
		{
			var windowArray = convolutionWindow.ToArray();

			return source
				.Window(windowArray.Length)
				.Select(w => w.Select((t, i) => t*windowArray[i]).Sum());
		}

		//Generates the convolution of the elements of the source 
		//generator with lists of values generated by another generator.
		public static IGenerator<float> Convolution(
			this IGenerator<float> source, 
			IGenerator<IList<float>> convolutionWindow, 
			int windowLength) 
			=> Generator.Combine(
				source.Window(windowLength),
				convolutionWindow,
				(w1, w2) => w1.Select((item, i) => item*w2[i]).Sum());

		//Generates lines "smooth" noise; essentially 
		//random linear ramps of the given number of samples.
		public static IGenerator<float> SmoothNoise(int sampleExponent, int smoothSampleCount)
		{
			/*
			int sampleCount = 1 << (sampleExponent + 1);

			var samples = Generator.UniformRandomFloat().Next(sampleCount);
			*/
			return Generator.UniformRandomFloat()
				.Interpolate(smoothSampleCount, Mathf.Lerp);
		}

		//Generates 1D Perlin noise.
		public static IGenerator<float> PerlinNoise(int levels, int sampleExponent)
		{
			//
			int totalSampleCount = 1 << sampleExponent; //16

			var smoothNoiseLayers = new List<IGenerator<float>>();

			for (int i = 0; i < levels; i++)
			{
				var smoothNoise = SmoothNoise(i, totalSampleCount / (1 << i));
				smoothNoiseLayers.Add(smoothNoise);
			}

			return Generator.Combine(smoothNoiseLayers, layers =>
			{
				float sum = 0f;
				float factorSum = 0f;

				for (int i = 0; i < layers.Count; i++)
				{
					float factor = 1f / (1 << i);

					sum += layers[i] * factor;
					factorSum += factor;
				}

				return sum / factorSum;
			});
		}
	}
}

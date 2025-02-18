﻿using UnityEngine.UI;
using Gamelogic.Extensions.Algorithms;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamelogic.Extensions.Samples
{
	/*	This class demonstrates a few examples of generators.
		There are more examples in the file
		ExampleGenerators.cs*/
	public class GeneratorsMain : GLMonoBehaviour
	{
		[Header("UI")]
		[ValidateNotNull]
		[SerializeField] private Text text = null;
		
		[ValidateNotNull]
		[SerializeField] private Dropdown dropDown = null;

		private IGenerator<string> generator;

		public void Start()
		{
			dropDown.AddOptions(new List<Dropdown.OptionData>
			{
				new Dropdown.OptionData("Uniform random float"),
				new Dropdown.OptionData("Count 0..9"),
				new Dropdown.OptionData("Random from list"),
				new Dropdown.OptionData("Primes"),
			});

			SelectGenerator(0);
		}

		public void OnNextButtonClicked()
		{
			text.text = generator.Next();
		}

		public void SelectGenerator(int index)
		{
			text.text = string.Empty;

			switch(index)
			{
				case 0:
					generator = Generator
						.UniformRandomFloat()
						.ElementsToString();
					break;
				case 1:
					generator = Generator
						.Count(10)
						.Select(x => x.ToString());
					break;

				case 2:
					var list = new List<int> { 2, 3, 5, 7, 11 };
					generator = Generator
						.ChooseUniformRandom(list)
						.ElementsToString();
					
					break;

				case 3:
					/*	This is a complex example
						Showing how to combine generators
						to generate primes.
					*/
					generator = Generator
						.Iterate(2, x => x + 1) //generates 2, 3, 4, 5, 6, 7, ...
						.Aggregate( //generates [2], [2, 3], [2, 3], [2, 3, 5], [2, 3, 5], [2, 3, 5, 7], ...
							(primes, n) =>
							{
								return primes
									.Where(prime => prime <= Mathf.Sqrt(n))
									.Any(prime => n % prime == 0) // Existing prime is a factor?
										? primes // No new primes
										: primes.Concat(new List<int> { n }).ToList(); // New prime
							},
							new List<int>())
						.Select(primes => primes.Last()) //generates 2, 3, 3, 5, 5, 7, ...
						.Window(2)// generates [2, 3], [3, 3], [3, 5], [5, 5], [5, 7]
						.Where(window => window[0] != window[1]) //generates [2, 3], [3, 5], [5, 7]
						.Select(window => window[0]) //generates 2, 3, 5, ...
						.ElementsToString(); //generates "2", "3", "5"
					break;
			}
		}		
	}

	public static class GeneratorsMainExtensions
	{
		/*	You can also define your own extension methods 
			for making new generators from existing ones.
			This method turns a generator that generates arbitrary 
			elements into a generator that generates the same elements 
			converted to strings using ToString
		*/
		public static IGenerator<string> ElementsToString<T>(this IGenerator<T> source)
			=> source.Select(x => x.ToString());

	}
}

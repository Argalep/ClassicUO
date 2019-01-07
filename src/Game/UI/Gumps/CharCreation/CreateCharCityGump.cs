﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO;

namespace ClassicUO.Game.UI.Gumps.CharCreation
{
	internal class CreateCharCityGump : Gump
	{
		internal class MapInfo
		{
			public int Index { get; set; }
			public string Name { get; set; }
			public Graphic Gump { get; set; }

			public int Width { get; set; }
			public int Height { get; set; }

			public int WidthOffset { get; set; }
			public int HeightOffset { get; set; }

			public MapInfo(int mapIndex, string name, Graphic gump, int width, int widthOffset, int height, int heightOffset)
			{
				Index = mapIndex;
				Name = name;
				Gump = gump;

				Width = width;
				Height = height;

				WidthOffset = widthOffset;
				HeightOffset = heightOffset;
			}
		}

		private static MapInfo[] _mapInfo =
		{
			new MapInfo(0, "Felucca",	5593, 0x1400, 0x0000, 0x1000, 0x0000),
			new MapInfo(1, "Trammel",	5594, 0x1400, 0x0000, 0x1000, 0x0000),
			new MapInfo(2, "Ilshenar",	5595, 0x0900, 0x0200, 0x0640, 0x0000),
			new MapInfo(3, "Malas",		5596, 0x0A00, 0x0000, 0x0800, 0x0000),
			new MapInfo(4, "Tokuno",	5597, 0x05A8, 0x0000, 0x05A8, 0x0000),
			new MapInfo(5, "Ter Mur",   5598, 0x0500, 0x0100, 0x1000, 0x0AC0),
		};

		private readonly PlayerMobile _character;

		private Label _mapName;
		private HtmlGump _description;
	
		private Dictionary<uint, CityCollection> _maps;

		private int _selectedMapIndex;

		private CityCollection _selectedMap;
		private CityInfo _selectedCity;

		public int SelectedMapIndex
		{
			get => _selectedMapIndex;
			set
			{
				_selectedMapIndex = value;

				if (_selectedMapIndex < 0)
					_selectedMapIndex = _maps.Count - 1;

				if (_selectedMapIndex >= _maps.Count)
					_selectedMapIndex = 0;

				SelectMap(_maps.ElementAt(_selectedMapIndex).Value);
			}
		}

		public CreateCharCityGump(PlayerMobile character) : base(0, 0)
		{
			_character = character;

			var loginScene = Engine.SceneManager.GetScene<LoginScene>();

			_maps = loginScene.Cities.GroupBy(city => city.Map)
				.ToDictionary(group => group.Key,
					group => new CityCollection(_mapInfo[group.Key], group.ToArray())
					{
						X = 57,
						Y = 49,
						OnSelect = SelectCity,
					}
				);

			SelectedMapIndex = 0;

			var mapCenterX = (393 / 2) + 57;

			AddChildren(new Button((int)Buttons.PreviousCollection, 0x15A1, 0x15A3, 0x15A2)
			{
				X = mapCenterX - 65,
				Y = 440,
				ButtonAction = ButtonAction.Activate
			});

			AddChildren(new Button((int)Buttons.NextCollection, 0x15A4, 0x15A6, 0x15A5)
			{
				X = mapCenterX + 50,
				Y = 440,
				ButtonAction = ButtonAction.Activate
			});

			AddChildren(new Button((int)Buttons.PreviousScreen, 0x15A1, 0x15A3, 0x15A2)
			{
				X = 586,
				Y = 435,
				ButtonAction = ButtonAction.Activate
			});

			AddChildren(new Button((int)Buttons.Finish, 0x15A4, 0x15A6, 0x15A5)
			{
				X = 610,
				Y = 435,
				ButtonAction = ButtonAction.Activate
			});
		}

		public override void OnButtonClick(int buttonID)
		{
			var charCreationGump = Engine.UI.GetByLocalSerial<CharCreationGump>();

			switch ((Buttons)buttonID)
			{
				case Buttons.PreviousScreen: charCreationGump.StepBack(); break;
				case Buttons.Finish:
					if (_selectedCity != null)
						charCreationGump.SetCity(_selectedCity);

					charCreationGump.CreateCharacter();
					break;

				case Buttons.PreviousCollection: SelectedMapIndex--; break;
				case Buttons.NextCollection: SelectedMapIndex++; break;
			}

			base.OnButtonClick(buttonID);
		}

		public void SelectCity(CityInfo city)
		{
			SelectCity(city.Index);
		}

		public void SelectCity(CitySelector selector)
		{
			SelectCity(selector.ButtonID);
		}

		public void SelectCity(int index)
		{
			if (_selectedMap != default(CityCollection))
			{
				var city = _selectedMap.FirstOrDefault(c => c.Index == index);

				if (city != default(CityInfo) && _selectedMap.Index == city.Map)
				{
					var selectors = GetSelectors();

					foreach (var s in selectors)
						s.IsSelected = false;

					var citySelector = selectors.FirstOrDefault(s => s.ButtonID == city.Index);

					if (citySelector != null)
						citySelector.IsSelected = true;

					_selectedCity = city;

					if (_description != null)
						RemoveChildren(_description);

					SetDescription(city);
				}
			}
		}

		private void SelectMap(CityCollection map)
		{
			if (_selectedMap != null)
				RemoveChildren(_selectedMap);

			_selectedMap = map;

			if (_selectedMap != null)
			{
				AddChildren(_selectedMap);

				if (_mapName != null)
					RemoveChildren(_mapName);

				var name = map.Name;
				var nameWidth = FileManager.Fonts.GetWidthASCII(3, name);

				AddChildren(_mapName = new Label(name, false, 1153, font: 3)
				{
					X = 57 + ((393 - nameWidth) / 2),
					Y = 440,
				});

				SelectCity(_selectedMap.FirstOrDefault());
			}
		}

		private void SetDescription(CityInfo info)
		{
			if (_description != null)
				RemoveChildren(_description);

			AddChildren((_description = new HtmlGump(452, 60, 173, 367, true, true, false,
				FileManager.Cliloc.GetString(info.Description), 0x000000, ishtml: true)));
		}

		private IEnumerable<CitySelector> GetSelectors()
		{
			foreach (var map in _maps.Values)
			{
				foreach (var selector in map.GetControls<CitySelector>())
					yield return selector;
			}
		}

		private enum Buttons
		{
			PreviousScreen,
			Finish,

			PreviousCollection,
			NextCollection,
		}


		internal class CityCollection : Gump, IEnumerable<CityInfo>
		{
			private MapInfo _mapInfo;
			private CityInfo[] _cities;

			public int Index => _mapInfo.Index;
			public string Name => _mapInfo.Name;

			public Action<CitySelector> OnSelect { get; set; }

			public CityCollection(MapInfo mapInfo, CityInfo[] cities) : base(0, 0)
			{
				_mapInfo = mapInfo;
				_cities = cities;

				AddChildren(new GumpPic(5, 5, _mapInfo.Gump, 0));
				AddChildren(new GumpPic(0, 0, 0x15DF, 0));

				var width = 393;
				var height = 393;

				var mapWidth = _mapInfo.Width - _mapInfo.WidthOffset;
				var mapHeight = _mapInfo.Height - _mapInfo.HeightOffset;

				var cityCount = cities.Length;

				for (int i = 0; i < cityCount; i++)
				{
					var city = cities[i];

					var buttonX = ((city.Position.X - _mapInfo.WidthOffset) * width / mapWidth);
					var buttonY = ((city.Position.Y - _mapInfo.HeightOffset) * height / mapHeight);

					var button = new Button(city.Index, 1209, 1210, 1210)
					{
						X = buttonX,
						Y = buttonY,
						ButtonAction = ButtonAction.Activate
					};

					var textX = buttonX;
					var textY = buttonY - 15;

					var cityName = city.City;
					var cityNameWidth = FileManager.Fonts.GetWidthASCII(3, cityName);

					var right = textX + cityNameWidth;
					var mapRight = width - 20;

					if (right > mapRight)
						textX -= right - mapRight;

					var label = new Label(cityName, false, 88, font: 3)
					{
						X = textX,
						Y = textY,
					};

					AddChildren(new CitySelector(button, label)
					{
						OnClick = (selector) => OnSelect(selector),
					});
				}
			}

			public IEnumerator<CityInfo> GetEnumerator()
			{
				foreach (var element in _cities)
					yield return element;
			}

			IEnumerator IEnumerable.GetEnumerator() => _cities.GetEnumerator();
		}

		public class CitySelector : Gump
		{
			private bool _isSelected;

			private Label _label;
			private Button _button;

			public int ButtonID => _button.ButtonID;

			public Action<CitySelector> OnClick { get; set; }

			public bool IsSelected
			{
				get => _isSelected;
				set
				{
					if (_isSelected != value)
					{
						_label.Hue = (ushort)(value ? 1153 : 88);
						_button.ButtonGraphicNormal = value ? 1210 : 1209;

						_isSelected = value;
					}
				}
			}

			public CitySelector(Button button, Label label) : base(0, 0)
			{
				AddChildren(_button = button);
				AddChildren(_label = label);
			}

			public override void Update(double totalMS, double frameMS)
			{
				base.Update(totalMS, frameMS);

				if (IsSelected)
					return;

				var controlUnderMouse = Engine.UI.MouseOverControl;

				if (Children.Contains(controlUnderMouse))
					_label.Hue = 153;
				else
					_label.Hue = 88;
			}

			public override void OnButtonClick(int buttonID)
			{
				if (_button.ButtonID == buttonID && OnClick != null)
					OnClick(this);
			}
		}
	}


}
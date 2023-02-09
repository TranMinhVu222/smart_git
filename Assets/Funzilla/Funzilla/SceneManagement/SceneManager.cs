
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

namespace Funzilla
{
	internal enum SceneOpenType
	{
		Single,
		Additive
	}

	internal class SceneManager : Singleton<SceneManager>
	{
		[SerializeField] private Image loadingScreen;
		[SerializeField] private SceneShield popupShield;
		[SerializeField] private LoadingShield loadingShield;

		private Transform _sceneNode;
		private Transform _popupNode;

		private enum ActionType
		{
			OpenScene,
			CloseScene,
			CloseScenes,
			PreloadScene,
			ReloadScene,
			ReloadScenes,
			OpenPopup,
			ClosePopup,
			ClosePopups,
		}

		private class Action
		{
			internal Action(ActionType type) { Type = type; }
			internal readonly ActionType Type;
		}

		// Scene action that's taking place
		private Action _action;

		private class SceneAction : Action
		{
			internal SceneAction(ActionType type, SceneID sceneId) : base(type)
			{
				SceneId = sceneId;
			}
			internal readonly SceneID SceneId;
		}

		private class SceneOpenAction : SceneAction
		{
			internal SceneOpenAction(SceneID sceneId, SceneOpenType openType, object data)
				: base(ActionType.OpenScene, sceneId)
			{
				OpenType = openType;
				Data = data;
			}
			internal readonly SceneOpenType OpenType;
			internal readonly object Data;
		}

		private class SceneReloadAction : SceneAction
		{
			internal SceneReloadAction(SceneID sceneId, object data) : base(ActionType.ReloadScene, sceneId)
			{
				Data = data;
			}
			internal readonly object Data;
			internal int Index;
		}

		private class PopupOpenAction : Action
		{
			internal PopupOpenAction(SceneID sceneId, object data) : base(ActionType.OpenPopup)
			{
				SceneId = sceneId;
				Data = data;
			}
			internal readonly SceneID SceneId;
			public readonly object Data;
		}

		// Node that stores inactive loaded scenes
		private Transform _pool;

		// Currently active scene
		private readonly List<Scene> _visibleScenes = new List<Scene>(4);

		// Currently active popups
		private readonly List<Popup> _visiblePopups = new List<Popup>(4);

		// Queued actions
		private readonly Queue<Action> _actions = new Queue<Action>(4);

		// Loaded scenes
		readonly Dictionary<string, SceneBase> _scenes = new Dictionary<string, SceneBase>(10);

		private static SceneID GetSceneID(string sceneId)
		{
			for (var i = 0; i < (int)SceneID.END; i++)
			{
				var sceneName = SceneNames.ScenesNameArray[i];
				if (sceneName.Equals(sceneId))
				{
					return (SceneID)i;
				}
			}
			return SceneID.END;
		}

		private void Awake()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
			_pool = new GameObject("pool").transform;
			_pool.SetParent(transform, false);
			_sceneNode = new GameObject("scenes").transform;
			_sceneNode.SetParent(transform, false);
			_popupNode = new GameObject("popups").transform;
			_popupNode.SetParent(transform, false);
			for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var activeScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				var scene = GetScene(activeScene);
				if (scene == null) continue;

				scene.ID = GetSceneID(activeScene.name);
				scene.name = activeScene.name;
				_scenes.Add(scene.name, scene);
				if (scene as Scene)
				{
					_visibleScenes.Add(scene as Scene);
					scene.gameObject.transform.SetParent(_sceneNode, false);
				}
				else
				{
					_visiblePopups.Add(scene as Popup);
					scene.gameObject.transform.SetParent(_popupNode, false);
				}

				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(activeScene);
				popupShield.transform.SetSiblingIndex(_sceneNode.GetSiblingIndex());
			}
		}

		internal static void OpenScene(SceneID sceneId, object data = null, SceneOpenType openType = SceneOpenType.Additive)
		{
			Instance._actions.Enqueue(new SceneOpenAction(sceneId, openType, data));
		}

		internal static void PreloadScene(SceneID sceneId)
		{
			Instance._actions.Enqueue(new SceneAction(ActionType.PreloadScene, sceneId));
		}

		internal static void CloseScene()
		{
			Instance._actions.Enqueue(new SceneAction(ActionType.CloseScene, SceneID.END));
		}

		internal static void CloseScene(SceneID sceneId)
		{
			Instance._actions.Enqueue(new SceneAction(ActionType.CloseScene, sceneId));
		}

		internal static void CloseScenes()
		{
			Instance._actions.Enqueue(new Action(ActionType.CloseScenes));
		}

		internal static void OpenPopup(SceneID sceneId, object data = null)
		{
			Instance._actions.Enqueue(new PopupOpenAction(sceneId, data));
		}

		internal static void ClosePopup()
		{
			Instance._actions.Enqueue(new Action(ActionType.ClosePopup));
		}

		internal static void ClosePopups()
		{
			Instance._actions.Enqueue(new Action(ActionType.ClosePopups));
		}

		internal static void ReloadScenes()
		{
			Instance._actions.Enqueue(new Action(ActionType.ReloadScenes));
		}

		internal static void ReloadScene(SceneID sceneId, object data = null)
		{
			Instance._actions.Enqueue(new SceneReloadAction(sceneId, data));
		}

		internal static void ShowLoading(
			bool loadingAnimationEnabled = true,
			float opacity = 0.7f,
			System.Action onComplete = null)
		{
			Instance.loadingShield.Show(loadingAnimationEnabled, opacity, onComplete);
		}

		internal static void HideLoading()
		{
			Instance.loadingShield.Hide();
		}

		private SceneBase GetLoadedScene(SceneID sceneId)
		{
			var sceneName = SceneNames.GetSceneName(sceneId);
			return _scenes.TryGetValue(sceneName, out var scene) ? scene : null;
		}

		private void ReloadScene(SceneID sceneId)
		{
			var sceneName = SceneNames.GetSceneName(sceneId);

			if (!_scenes.TryGetValue(sceneName, out var scene) || !scene.gameObject.activeSelf)
			{ // Nothing happen as the scene is not active
				_action = null;
				return;
			}

			var sceneReloadAction = (SceneReloadAction)_action;
			if (scene.AliveAfterClose)
			{
				scene.Init(sceneReloadAction.Data);
			}
			else
			{
				sceneReloadAction.Index = scene.transform.GetSiblingIndex();
				_visibleScenes.Remove((Scene)scene);
				_scenes.Remove(sceneName);
				Destroy(scene.gameObject);
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
					sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
			}
		}

		private void LoadScene(SceneID sceneId)
		{
			var sceneName = SceneNames.GetSceneName(sceneId);

			if (_scenes.TryGetValue(sceneName, out var scene))
			{
				if (!scene.gameObject.activeSelf)
				{
					scene.gameObject.SetActive(true);
					OnSceneLoaded(scene);
				}
				else
				{
					_action = null;
				}
			}
			else
			{
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
					sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
			}
		}

		private void OnSceneLoaded(SceneBase scene)
		{
			if (_action == null)
			{
				return;
			}

			switch (_action.Type)
			{
				case ActionType.OpenScene:
					var sceneOpenAction = (SceneOpenAction)_action;
					if (sceneOpenAction.OpenType == SceneOpenType.Single)
					{
						foreach (var s in _visibleScenes)
						{
							if (!s.AliveAfterClose)
							{
								_scenes.Remove(s.name);
								Destroy(s.gameObject);
							}
							else
							{
								s.transform.SetParent(_pool, false);
								s.gameObject.SetActive(false);
							}
						}
						_visibleScenes.Clear();
					}
					scene.transform.SetParent(_sceneNode, false);
					scene.ID = sceneOpenAction.SceneId;
					_visibleScenes.Add((Scene)scene);
					scene.Init(sceneOpenAction.Data);
					scene.AnimateIn();
					break;

				case ActionType.PreloadScene:
					var sceneAction = (SceneAction)_action;
					scene.transform.SetParent(_pool, false);
					scene.gameObject.SetActive(false);
					scene.ID = sceneAction.SceneId;
					_action = null;
					break;

				case ActionType.ReloadScene:
					var sceneReloadAction = (SceneReloadAction)_action;
					scene.transform.SetParent(_sceneNode, false);
					scene.ID = sceneReloadAction.SceneId;
					_visibleScenes.Insert(sceneReloadAction.Index, (Scene)scene);
					scene.Init(sceneReloadAction.Data);
					scene.AnimateIn();
					break;

				case ActionType.OpenPopup:
					scene.transform.SetParent(_popupNode, false);
					var popupOpenAction = (PopupOpenAction)_action;
					scene.ID = popupOpenAction.SceneId;
					_visiblePopups.Add((Popup)scene);
					scene.Init(popupOpenAction.Data);
					scene.AnimateIn();
					break;
				case ActionType.CloseScene:
					break;
				case ActionType.CloseScenes:
					break;
				case ActionType.ReloadScenes:
					break;
				case ActionType.ClosePopup:
					break;
				case ActionType.ClosePopups:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnSceneLoaded(
			UnityEngine.SceneManagement.Scene loadedScene,
			UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			OnSceneLoaded(loadedScene);
		}

		private void ConsumeScene(SceneBase scene)
		{
			if (scene.AliveAfterClose)
			{
				scene.gameObject.SetActive(false);
				scene.transform.SetParent(_pool, false);
			}
			else
			{
				_scenes.Remove(scene.name);
				Destroy(scene.gameObject);
			}
		}

		internal static void OnSceneAnimatedOut(SceneBase scene)
		{
			var instance = Instance;
			switch (Instance._action.Type)
			{
				case ActionType.OpenScene:
					break;
				case ActionType.CloseScene:
					instance.ConsumeScene(scene);
					instance._action = null;
					break;
				case ActionType.OpenPopup:
					{
						scene.gameObject.SetActive(false);
						var sceneId = ((PopupOpenAction)instance._action).SceneId;
						instance.LoadScene(sceneId);
					}
					break;
				case ActionType.ReloadScenes:
					break;
				case ActionType.ClosePopup:
					instance.ConsumeScene(scene);
					if (instance._visiblePopups.Count > 0)
					{ // Let the last popup appear
						var popup = instance._visiblePopups.Last();
						popup.gameObject.SetActive(true);
						popup.AnimateIn();
					}
					else
					{ // No popup active, hide the shield now
						instance._action = null;
					}
					break;
				case ActionType.ClosePopups:
					instance.ConsumeScene(scene);
					foreach (var popup in instance._visiblePopups)
					{
						instance.ConsumeScene(popup);
					}
					instance._visiblePopups.Clear();
					instance.popupShield.Hide();
					instance._action = null;
					break;
				case ActionType.CloseScenes:
					break;
				case ActionType.PreloadScene:
					break;
				case ActionType.ReloadScene:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal static void HideSplash()
		{
			var instance = Instance;
			if (instance.loadingScreen != null && instance.loadingScreen.transform.parent.gameObject.activeSelf)
			{
				instance.loadingScreen.DOFade(0, 0.2f).onComplete = () =>
				{
					instance.loadingScreen.transform.parent.gameObject.SetActive(false);
				};
			}
		}

		internal static void OnSceneAnimatedIn(SceneBase scene)
		{
			Instance._action = null;
		}

		private static SceneBase GetScene(UnityEngine.SceneManagement.Scene loadedScene)
		{
			SceneBase scene = null;
			foreach (var obj in loadedScene.GetRootGameObjects())
			{
				scene = obj.GetComponent<SceneBase>();
				if (scene == null)
				{
					scene = obj.GetComponentInChildren<SceneBase>();
				}
				if (scene != null)
				{
					break;
				}
			}
			return scene;
		}

		private void OnSceneLoaded(UnityEngine.SceneManagement.Scene loadedScene)
		{
			var scene = GetScene(loadedScene);
			foreach (var obj in loadedScene.GetRootGameObjects())
			{
				scene = obj.GetComponent<SceneBase>();
				if (scene == null)
				{
					scene = obj.GetComponentInChildren<SceneBase>();
				}
				if (scene != null)
				{
					break;
				}
			}

			if (scene == null) return;
			scene.name = loadedScene.name;
			_scenes.Add(loadedScene.name, scene);

			OnSceneLoaded(scene);
			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(loadedScene);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (_visiblePopups.Count > 0)
				{
					_visiblePopups.Last().OnBackButtonPressed();
				}
				else if (_visibleScenes.Count > 0)
				{
					_visibleScenes.Last().OnBackButtonPressed();
				}
			}
			if (_action != null || _actions.Count <= 0)
			{
				return;
			}

			_action = _actions.Dequeue();
			switch (_action.Type)
			{
				case ActionType.OpenScene:
					{
						var sceneOpenAction = (SceneOpenAction)_action;
						var sceneId = sceneOpenAction.SceneId;
						LoadScene(sceneId);
					}
					break;
				case ActionType.PreloadScene:
					{
						var sceneAction = (SceneAction)_action;
						var sceneId = sceneAction.SceneId;
						LoadScene(sceneId);
					}
					break;
				case ActionType.CloseScene:
					{
						var sceneAction = (SceneAction)_action;
						if (sceneAction.SceneId == SceneID.END)
						{
							if (_visibleScenes.Count <= 0)
							{
								return;
							}
							var scene = _visibleScenes.Last();
							_visibleScenes.RemoveAt(_visibleScenes.Count - 1);
							scene.AnimateOut();
						}
						else
						{
							var sceneBase = GetLoadedScene(sceneAction.SceneId);
							if (sceneBase)
							{
								var scene = sceneBase.gameObject.GetComponent<Scene>();
								if (scene)
								{
									_visibleScenes.Remove(scene);
									scene.AnimateOut();
									return;
								}
							}
							_action = null;
						}
					}
					break;
				case ActionType.CloseScenes:
					foreach (var scene in _visibleScenes)
					{
						ConsumeScene(scene);
					}
					_visibleScenes.Clear();
					_action = null;
					break;
				case ActionType.ReloadScene:
					{
						ReloadScene(((SceneReloadAction)_action).SceneId);
					}
					break;
				case ActionType.ReloadScenes:
					{
						foreach (var scene in _visibleScenes)
						{
							OpenScene(scene.ID);
							ConsumeScene(scene);
						}
						_visibleScenes.Clear();
						_action = null;
					}
					break;
				case ActionType.OpenPopup:
					{
						var sceneId = ((PopupOpenAction)_action).SceneId;
						var scene = GetLoadedScene(sceneId);
						if (scene && scene.gameObject.activeSelf)
						{ // Should not open a popup already opened
							_action = null;
							return;
						}
						if (_visiblePopups.Count > 0)
						{ // Hide the current active popup first
							_visiblePopups.Last().AnimateOut();
						}
						else
						{
							popupShield.Show();
							LoadScene(sceneId);
						}
					}
					break;
				case ActionType.ClosePopups:
				case ActionType.ClosePopup:
					{
						if (_visiblePopups.Count <= 0)
						{
							_action = null;
							return;
						}
						var popup = _visiblePopups.Last();
						_visiblePopups.RemoveAt(_visiblePopups.Count - 1);
						popup.AnimateOut();
						if (_visiblePopups.Count <= 0)
						{
							popupShield.Hide();
						}
					}
					break;
			}
		}
	}
}
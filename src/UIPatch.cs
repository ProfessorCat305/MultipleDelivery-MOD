using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using System;
using static MultipleDelivery_MOD.src.DispenserPatch;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEngine.EventSystems;

namespace MultipleDelivery_MOD.src
{
    internal static class UIDispenserWindowPatch
    {
        private static GameObject _obj;

        private static GameObject _iconObj;

        private static Text _text;

        private static Text _count;

        private static Image _icon;


        private static UIFilter[] _uiFilter = new UIFilter[5];

        private static Button _button;

        private static Sprite _tagNotSelectedSprite;

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        public static void VFPreload_InvokeOnLoadWorkEnded_Postfix()
        {
            bool flag = UIDispenserWindowPatch._obj == null;
            if (flag) {
                GameObject dispenserWindow = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window");
                RectTransform windowTrans = dispenserWindow.GetComponent<RectTransform>();
                windowTrans.sizeDelta = new Vector2(800f, 430f);


                Transform transform1 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/warning-tip-box").transform;
                transform1.localPosition = new Vector3(-340f, -87f, 0f);

                Transform transform = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid").transform;
                transform.localPosition = new Vector3(340f, -56f, 0f);
                UIDispenserWindowPatch._obj = new GameObject {
                    name = "multiItem"
                };
                UIDispenserWindowPatch._obj.transform.SetParent(transform, false);
                UIDispenserWindowPatch._obj.transform.localPosition = new Vector3(-360f, -8f, 0f);
                Transform transform2 = UIDispenserWindowPatch._obj.transform;
                GameObject original = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-empty");
                GameObject original2 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon");
                //GameObject original3 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon/current-count-text");
                //GameObject original4 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon/reset-btn");
                //GameObject original5 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon/inc-1");
                //GameObject original6 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon/inc-2");
                //GameObject original7 = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dispenser Window/panel-mid/filter-icon/inc-3");

                for (int i = 0; i < 5; i++) {
                    GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(original, transform2);
                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original2, transform2);
                    gameObject1.transform.localScale = Vector3.one;
                    gameObject1.transform.localPosition = new Vector3(-70f * (i + 1), 0f, 0f);
                    gameObject2.transform.localScale = Vector3.one;
                    gameObject2.transform.localPosition = new Vector3(-70f * (i + 1), 0f, 0f);
                    _uiFilter[i] = new UIFilter(i, gameObject1, gameObject2, dispenserWindow.transform);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIDispenserWindow), "OnDispenserIdChange")]
        [HarmonyPatch(typeof(UIDispenserWindow), "_OnUpdate")]
        public static void UIDispenserWindow_OnDispenserIdChange_Postfix(ref UIDispenserWindow __instance)
        {
            if (__instance.active) {
                if (__instance.dispenserId == 0 || __instance.factory == null) {
                    __instance._Close();
                    return;
                }
                DispenserComponent dispenserComponent = __instance.transport.dispenserPool[__instance.dispenserId];
                if (dispenserComponent == null || dispenserComponent.id != __instance.dispenserId) {
                    __instance._Close();
                    return;
                }
                //EventSystem.current.SetSelectedGameObject(null);
                ItemProto itemProto = LDB.items.Select((int)__instance.factory.entityPool[dispenserComponent.entityId].protoId);
                if (itemProto == null) {
                    __instance._Close();
                    return;
                }
                for (int i = 0; i < 5; i++) {
                    _uiFilter[i].OnUpdate(dispenserComponent, __instance.factory, __instance.dispenserId);
                }
            }
        }
    }

    public class UIFilter
    {
        private int filterIndex;

        private Transform _baseTransform;

        private UIButton _uiEmptyButton; // Removed static modifier to make it an instance field
        private UIButton _uiFilterButton;
        private Image itemImage;
        private UIButton _uiTakeBackButton;
        private Text countText;
        private Text orderCountText;
        private Image incImage1;
        private Image incImage2;
        private Image incImage3;

        private int LocalDispenserId;
        private int LocalPlanetId;

        public UIFilter(int index, GameObject gameObject1, GameObject gameObject2, Transform baseTransform)
        {
            filterIndex = index;
            _uiEmptyButton = gameObject1.GetComponent<UIButton>();
            //_uiButton.transform.GetChild(1).gameObject.SetActive(false);
            _uiEmptyButton.onClick += OnSelectItemButtonClick; // Updated to use instance method  

            _uiFilterButton = gameObject2.GetComponent<UIButton>();
            itemImage = gameObject2.GetComponent<Image>();
            _uiTakeBackButton = _uiFilterButton.transform.GetChild(2).GetComponent<UIButton>();
            _uiTakeBackButton.onClick += OnTakeBackButtonClick; // Updated to use instance method
            countText = gameObject2.transform.Find("current-count-text").GetComponent<Text>();
            //countText = _uiFilterButton.transform.GetChild(0).GetComponent<Text>();
            orderCountText = gameObject2.transform.Find("ordered-count-text").GetComponent<Text>();
            incImage1 = _uiFilterButton.transform.GetChild(3).GetComponent<Image>();
            incImage2 = _uiFilterButton.transform.GetChild(4).GetComponent<Image>();
            incImage3 = _uiFilterButton.transform.GetChild(5).GetComponent<Image>();

            _baseTransform = baseTransform;
        }

        public void OnUpdate(DispenserComponent dispenserComponent, PlanetFactory factory, int dispenserId)
        {
            LocalDispenserId = dispenserId;
            LocalPlanetId = factory.planet.id;

            Dictionary<int, int[]> MutiFilterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(LocalPlanetId);
            int[] filterData = MutiFilterdata[LocalDispenserId];
            
            int itemId = filterData[filterIndex];
            if (itemId > 0) {
                ItemProto itemProto = LDB.items.Select(itemId);
                if (itemProto != null) {
                    _uiFilterButton.tips.itemId = itemId;
                    itemImage.sprite = itemProto.iconSprite;
                    _uiEmptyButton.gameObject.SetActive(false);
                    _uiFilterButton.gameObject.SetActive(true);
                }
            } else {
                _uiEmptyButton.gameObject.SetActive(true);
                _uiFilterButton.gameObject.SetActive(false);
            }

            int count;
            int inc;
            CalculateStorageTotalCount(dispenserComponent, itemId, out count, out inc);
            countText.text = count.ToString();
            int num3 = (count <= 0 || inc <= 0) ? 0 : (inc / count);
            incImage1.enabled = (num3 >= 1 && num3 < 2);
            incImage2.enabled = (num3 >= 2 && num3 < 4);
            incImage3.enabled = (num3 >= 4);

            int storageOrdered = GetItemStorageOrdered(dispenserComponent, factory, itemId);
            int playerOrdered = GetItemPlayerOrdered(dispenserComponent, factory, itemId);

            int num5 = playerOrdered + storageOrdered;
            if (num5 == 0) {
                orderCountText.text = "";
                //if (this.orderedCountLabel != null) {
                //    this.orderedCountLabel.color = this.orderedNormalColor;
                //}
                //if (orderCountText != null) {
                //    orderCountText.color = this.orderedNormalColor;
                //}
            } else if (num5 < 0) {
                orderCountText.text = num5.ToString();
                //if (this.orderedCountLabel != null) {
                //    this.orderedCountLabel.color = this.orderedNagativeColor;
                //}
                //if (orderCountText != null) {
                //    orderCountText.color = this.orderedNagativeColor;
                //}
            } else {
                orderCountText.text = "+" + num5.ToString();
                //if (this.orderedCountLabel != null) {
                //    this.orderedCountLabel.color = this.orderedPositiveColor;
                //}
                //if (orderCountText != null) {
                //    orderCountText.color = this.orderedPositiveColor;
                //}
            }
        }

        // 打开物品选择器
        public void OnSelectItemButtonClick(int obj)
        {
            if (UIItemPicker.isOpened) {
                UIItemPicker.Close();
                return;
            }
            UIItemPicker.Popup((_baseTransform as RectTransform).anchoredPosition + new Vector2(-300f, 0f), new Action<ItemProto>(this.OnItemPickerReturn));
        }
        public void OnItemPickerReturn(ItemProto itemProto)
        {
            if (itemProto == null) {
                return;
            }
            if (LocalDispenserId == 0 || LocalPlanetId == 0) {
                return;
            }
            if (GameMain.localPlanet.id != LocalPlanetId) {
                return;
            }
            PlanetTransport transport = GameMain.localPlanet.factory.transport;
            DispenserComponent dispenserComponent = transport.dispenserPool[LocalDispenserId];
            if (dispenserComponent == null || dispenserComponent.id != LocalDispenserId) {
                return;
            }
            if (dispenserComponent.filter == itemProto.ID && itemProto.ID != 0) {
                return;
            }
            DispenserMutiFilterManager.Instance.SetDispenserFilter(LocalPlanetId, LocalDispenserId, filterIndex, itemProto.ID);
            transport.RefreshDispenserTraffic(LocalDispenserId);
            OnUpdate(dispenserComponent, transport.factory, LocalDispenserId);
        }

        // 清空筛选过滤器
        public void OnTakeBackButtonClick(int obj)
        {
            if (LocalDispenserId == 0 || LocalPlanetId == 0) {
                return;
            }
            PlanetTransport transport = GameMain.localPlanet.factory.transport;
            DispenserComponent dispenserComponent = transport.dispenserPool[LocalDispenserId];
            if (dispenserComponent == null || dispenserComponent.id != LocalDispenserId) {
                return;
            }
            DispenserMutiFilterManager.Instance.SetDispenserFilter(LocalPlanetId, LocalDispenserId, filterIndex, 0);
            transport.RefreshDispenserTraffic(LocalDispenserId);
            itemImage.sprite = null;
            _uiEmptyButton.gameObject.SetActive(true);
            _uiFilterButton.gameObject.SetActive(false);
            //this.pointerInIcon = false;
        }

        

        // 读取箱子内物品数量
        private void CalculateStorageTotalCount(DispenserComponent dispenserComponent, int itemId, out int count, out int inc)
        {
            count = 0;
            inc = 0;
            if (dispenserComponent.storage != null && itemId > 0) {
                StorageComponent storageComponent = dispenserComponent.storage;
                do {
                    int num;
                    count += storageComponent.GetItemCount(itemId, out num);
                    inc += num;
                    storageComponent = storageComponent.previousStorage;
                }
                while (storageComponent != null);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;


public class Slot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private ItemType? allowedItemType = null; // null̸  Ÿ
    [SerializeField] private bool isEquipSlot = false;

    private Item _item;
    private Player _player;
    private ItemSlotHandler slotHandler;
    private StatUI statUI;
    private WeaponHolder weaponHolder;

    public Item Item => _item;
    public ItemType? AllowedItemType => allowedItemType;

    public void Initialize(Player player, InventoryUI inventory, StatUI statUI)
    {
        _player = player;
        itemIcon.gameObject.SetActive(false);
        this.statUI = statUI;
        weaponHolder = player.GetComponent<WeaponHolder>();
        slotHandler = GetComponent<ItemSlotHandler>();
        if (slotHandler != null)
        {
            InitializeSlotHandler(inventory);
        }
    }

    private void InitializeSlotHandler(InventoryUI inventory)
    {
        slotHandler.IsInventorySlot = !isEquipSlot;

        slotHandler.InitializeSlotHandler(inventory);
    }

    public bool IsEmpty() => _item == null;

    public bool CanAcceptItem(Item item)
    {
        if (item == null) return false;

        if (isEquipSlot && allowedItemType.HasValue)
        {
            if (allowedItemType.Value == ItemType.Weapon)
            {
                return item.ItemType == ItemType.Sword || item.ItemType == ItemType.Bow;
            }

            return item.ItemType == allowedItemType.Value;
        }

        return true;
    }

    public void PlaceItem(Item item)
    {
        if (item != null && !CanAcceptItem(item)) return;

        // 현재 체력/마나 비율 저장 (1을 넘지 않도록)
        float healthRatio = Mathf.Min(1f, _player.CharacterStats.GetStatValue(StatType.Health) /
                                     _player.CharacterStats.GetStatValue(StatType.MaxHealth));
        float manaRatio = Mathf.Min(1f, _player.CharacterStats.GetStatValue(StatType.Mana) /
                                   _player.CharacterStats.GetStatValue(StatType.MaxMana));

        Debug.Log($"[장비 장착 전] HP: {_player.CharacterStats.GetStatValue(StatType.Health)}/{_player.CharacterStats.GetStatValue(StatType.MaxHealth)} ({healthRatio:P0}), " +
                  $"MP: {_player.CharacterStats.GetStatValue(StatType.Mana)}/{_player.CharacterStats.GetStatValue(StatType.MaxMana)} ({manaRatio:P0})");

        RemoveCurrentItem();
        _item = item;

        if (_item != null)
        {
            itemIcon.sprite = _item.Icon;
            itemIcon.gameObject.SetActive(true);

            if (isEquipSlot && _player != null)
            {
                // 먼저 모든 모디파이어 제거
                _player.CharacterStats.GetStat(StatType.Health).ClearAllModifiers();
                _player.CharacterStats.GetStat(StatType.Mana).ClearAllModifiers();

                _item.ApplyStats(_player.GetComponent<ICharacterStats>());
                _player.UpdateMoveSpeed();

                // 새로운 최대값으로 현재 체력/마나 설정 (비율 유지)
                float newMaxHealth = _player.CharacterStats.GetStatValue(StatType.MaxHealth);
                float newMaxMana = _player.CharacterStats.GetStatValue(StatType.MaxMana);

                ((FloatStat)_player.CharacterStats.GetStat(StatType.Health)).SetBaseValue(newMaxHealth * healthRatio);
                ((FloatStat)_player.CharacterStats.GetStat(StatType.Mana)).SetBaseValue(newMaxMana * manaRatio);

                Debug.Log($"[장비 장착 후] HP: {_player.CharacterStats.GetStatValue(StatType.Health)}/{newMaxHealth} ({healthRatio:P0}), " +
                          $"MP: {_player.CharacterStats.GetStatValue(StatType.Mana)}/{newMaxMana} ({manaRatio:P0})");

                if (_item.ItemType == ItemType.Sword || _item.ItemType == ItemType.Bow)
                {
                    weaponHolder.EquipWeapon(_item.ItemData.ItemPrefab, _item.ItemType);

                    Skill defaultSkill = _item.ItemData.GetDefaultSkillForWeapon();
                    if (defaultSkill != null)
                    {
                        _player.skillController.DirectEquipSkill(defaultSkill, KeyCode.Mouse0);
                    }

                    _player.AnimController.PlayerAnimatorChange(_item.ItemType);
                }
                if (_item.ItemType == ItemType.Armor)
                {
                    foreach (EquipmentIndex eIndex in _player.InventorySystem.equipmentIndices)
                    {
                        if (eIndex.itemID == _item.ItemID)
                        {
                            foreach (GameObject go in eIndex.objsToAcitve)
                            {
                                go.SetActive(true);
                            }
                        }
                    }
                }
                if (_item.ItemType == ItemType.Pet)
                {
                    Player player = GameManager.Instance.player;
                    Pet pet = Instantiate(_item.ItemPrefab, player.transform.position, Quaternion.identity).GetComponent<Pet>();
                    pet.Initialize(player);
                    player.pet = pet;
                }
            }
        }
        else
        {
            itemIcon.gameObject.SetActive(false);
        }

        statUI.UpdateUI();
    }

    private void RemoveCurrentItem()
    {
        if (_item != null && isEquipSlot && _player != null)
        {
            float healthRatio = Mathf.Min(1f, _player.CharacterStats.GetStatValue(StatType.Health) /
                                        _player.CharacterStats.GetStatValue(StatType.MaxHealth));
            float manaRatio = Mathf.Min(1f, _player.CharacterStats.GetStatValue(StatType.Mana) /
                                      _player.CharacterStats.GetStatValue(StatType.MaxMana));


            _player.CharacterStats.GetStat(StatType.Health).ClearAllModifiers();
            _player.CharacterStats.GetStat(StatType.Mana).ClearAllModifiers();

            _item.RemoveStats(_player.GetComponent<ICharacterStats>());
            _player.UpdateMoveSpeed();

            float newMaxHealth = _player.CharacterStats.GetStatValue(StatType.MaxHealth);
            float newMaxMana = _player.CharacterStats.GetStatValue(StatType.MaxMana);

            ((FloatStat)_player.CharacterStats.GetStat(StatType.Health)).SetBaseValue(newMaxHealth * healthRatio);
            ((FloatStat)_player.CharacterStats.GetStat(StatType.Mana)).SetBaseValue(newMaxMana * manaRatio);


            if (_item.ItemType == ItemType.Sword || _item.ItemType == ItemType.Bow)
            {
                _player.skillController.RemoveAllWeaponSkills();
                _player.AnimController.PlayerAnimatorChange(ItemType.None);
                weaponHolder.UnequipCurrentWeapon();
            }

            if (_item.ItemType == ItemType.Pet)
            {
                Player player = GameManager.Instance.player;
                if (player.pet != null)
                {
                    Destroy(player.pet.gameObject);
                    player.pet = null;
                }
            }
            if (_item.ItemType == ItemType.Armor)
            {
                foreach (EquipmentIndex eIndex in _player.InventorySystem.equipmentIndices)
                {
                    if (eIndex.itemID == _item.ItemID)
                    {
                        foreach (GameObject go in eIndex.objsToAcitve)
                        {
                            go.SetActive(false);
                        }
                    }
                }

            }

        }

    }

    public void PlaceholdItem(Item item)
    {
        if (!CanAcceptItem(item)) return;

        itemIcon.sprite = item.Icon;
        itemIcon.gameObject.SetActive(true);
        itemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void ClearSlot()
    {
        RemoveCurrentItem();
        _item = null;
        itemIcon.gameObject.SetActive(false);
        itemIcon.color = Color.white;
        statUI.UpdateUI();
    }

    public void SetAsEquipmentSlot(ItemType type)
    {
        isEquipSlot = true;
        allowedItemType = type;

        if (slotHandler != null)
        {
            slotHandler.IsInventorySlot = false;
        }
    }
}

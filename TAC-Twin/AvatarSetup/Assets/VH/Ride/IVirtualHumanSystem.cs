using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHAssets;
using Ride.Conversation;

namespace Ride.VirtualHumans
{
    /// <summary>
    /// Interface for a Ride system that handles Virtual Human Creation, Deletion, Config Save/Load and conversations
    /// </summary>
    public interface IVirtualHumanSystem : IRideSystem
    {
        ICharacter CreateVHCharacterInView(
            string characterName,
            string characterTemplate,
            Vector3 viewerPos,
            Vector3 viewerForward,
            ConversationContext context = null);

        ICharacter CreateVHCharacterAtPostion(
            string characterName,
            string characterTemplate,
            Vector3 position,
            Vector3 forward = default,
            ConversationContext context = null);

        void DeleteVHCharacter(string characterName);
        void DeleteVHCharacter(MecanimCharacter character);
        void RegisterVHCharacter(MecanimCharacter character, ConversationContext context = null);
        void RemoveVHCharacter(MecanimCharacter character);
        void SaveVHCharacterConfig(ICharacter character, List<VHConfigSerializer.VHSystemID> systemIDs);
        VHConfigSerializer.VirtualHumanConfig LoadCharacterConfig(string name);
        ConversationContext GetConversationContext(string virutalHumanName);
        ConversationContext GetConversationContext();
        ConversationHandler GetConversationHandler(string virutalHumanName);
        ConversationHandler GetConversationHandler();

        VHConfigSerializer Serializer { get; }
        VHConversationManager ConversationManager { get; }
        List<string> CharacterTemplates { get; }
        string LookupVHCharacterTemplate(string name);
    }
}

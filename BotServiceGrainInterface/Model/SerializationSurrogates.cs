using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotServiceGrainInterface.Model
{
    [GenerateSerializer]
    public struct HelixChannelInfoSurrogate
    {
        [Id(0)]
        public string BroadcasterId { get; set; }

        [Id(1)]
        public string BroadcasterName { get; set; }

        [Id(2)]
        public string BroadcasterLanguage { get; set; }

        [Id(3)]
        public string GameId { get; set; }

        [Id(4)]
        public string GameName { get; set; }

        [Id(5)]
        public string Title { get; set; }
    }

    [GenerateSerializer]
    public struct ChannelStaffSurrogate
    {
        [Id(0)]
        public HelixChannelEditor[] Editors { get; set; }

        [Id(1)]
        public HelixChannelModerator[] Moderators { get; set; }
    }

    [GenerateSerializer]
    public struct HelixChannelEditorSurrogate
    {
        [Id(0)]
        public string UserId { get; set; }

        [Id(1)]
        public string UserName { get; set; }

        [Id(2)]
        public DateTimeOffset CreatedAt { get; set; }
    }

    [GenerateSerializer]
    public struct HelixChannelModeratorSurrogate
    {
        [Id(0)]
        public string UserId { get; set; }

        [Id(1)]
        public string UserLogin { get; set; }

        [Id(2)]
        public string UserName { get; set; }
    }

    [GenerateSerializer]
    public struct CommandOptionsSurrogate
    {
        [Id(0)]
        public Guid Id { get; set; }

        [Id(1)]
        public string[] Aliases { get; set; }

        [Id(2)]
        public string Type { get; set; }

        [Id(3)]
        public Dictionary<string, string> Parameters { get; set; }
    }

    [GenerateSerializer]
    public struct CommandMetadataSurrogate
    {
        [Id(0)]
        public string Name { get; set; }

        [Id(1)]
        public string Description { get; set; }
    }

    [GenerateSerializer]
    public struct BotAccountInfoSurrogate
    {
        [Id(0)]
        public bool IsActive { get; set; }

        [Id(1)]
        public string UserId { get; set; }

        [Id(2)]
        public string UserLogin { get; set; }
    }

    [GenerateSerializer]
    public struct CustomCategoryDescriptionSurrogate
    {
        [Id(0)]
        public string TwitchCategoryId { get; set; }
        
        [Id(1)]
        public string Locale { get; set; }

        [Id(2)]
        public string Description { get; set; }
    }

    [GenerateSerializer]
    public struct UserRoleSurrogate
    {
        [Id(0)]
        public ChannelRole Role { get; set; }

        [Id(1)]
        public string ChannelId { get; set; }

        [Id(2)]
        public string ChannelName { get; set; }
    }

    [RegisterConverter]
    public sealed class SurrogatesConverter
        : IConverter<HelixChannelInfo, HelixChannelInfoSurrogate>
        , IConverter<ChannelStaff, ChannelStaffSurrogate>
        , IConverter<HelixChannelEditor, HelixChannelEditorSurrogate>
        , IConverter<HelixChannelModerator, HelixChannelModeratorSurrogate>
        , IConverter<CommandOptions, CommandOptionsSurrogate>
        , IConverter<CommandMetadata, CommandMetadataSurrogate>
        , IConverter<BotAccountInfo, BotAccountInfoSurrogate>
        , IConverter<CustomCategoryDescription, CustomCategoryDescriptionSurrogate>
        , IConverter<UserRole, UserRoleSurrogate>
    {
        public HelixChannelInfo ConvertFromSurrogate(in HelixChannelInfoSurrogate s) =>
            new()
            {
                BroadcasterId = s.BroadcasterId,
                BroadcasterName = s.BroadcasterName,
                BroadcasterLanguage = s.BroadcasterLanguage,
                GameId = s.GameId,
                GameName = s.GameName,
                Title = s.Title,
            };

        public HelixChannelInfoSurrogate ConvertToSurrogate(in HelixChannelInfo i) =>
            new()
            {
                BroadcasterId = i.BroadcasterId,
                BroadcasterName = i.BroadcasterName,
                BroadcasterLanguage = i.BroadcasterLanguage,
                GameId = i.GameId,
                GameName = i.GameName,
                Title = i.Title,
            };
        public ChannelStaff ConvertFromSurrogate(in ChannelStaffSurrogate s) =>
            new()
            {
                Editors = s.Editors,
                Moderators = s.Moderators,
            };

        public ChannelStaffSurrogate ConvertToSurrogate(in ChannelStaff i) =>
            new()
            {
                Editors = i.Editors,
                Moderators = i.Moderators,
            };
        public HelixChannelEditor ConvertFromSurrogate(in HelixChannelEditorSurrogate s) =>
            new()
            {
                CreatedAt = s.CreatedAt,
                UserId = s.UserId,
                UserName = s.UserName,
            };

        public HelixChannelEditorSurrogate ConvertToSurrogate(in HelixChannelEditor i) =>
            new()
            {
                UserName = i.UserName,
                UserId = i.UserId,
                CreatedAt = i.CreatedAt,
            };

        public HelixChannelModerator ConvertFromSurrogate(in HelixChannelModeratorSurrogate s) =>
            new()
            {
                UserId = s.UserId,
                UserLogin = s.UserLogin,
                UserName = s.UserName,
            };

        public HelixChannelModeratorSurrogate ConvertToSurrogate(in HelixChannelModerator i) =>
            new()
            {
                UserId = i.UserId,
                UserLogin = i.UserLogin,
                UserName = i.UserName,
            };

        public CommandOptions ConvertFromSurrogate(in CommandOptionsSurrogate s) =>
            new()
            {
                Id = s.Id,
                Aliases = s.Aliases,
                Parameters = s.Parameters,
                Type = s.Type
            };

        public CommandOptionsSurrogate ConvertToSurrogate(in CommandOptions i) =>
            new()
            {
                Id = i.Id,
                Aliases = i.Aliases,
                Parameters = i.Parameters,
                Type = i.Type,
            };

        public CommandMetadata ConvertFromSurrogate(in CommandMetadataSurrogate s) =>
            new()
            {
                Description = s.Description,
                Name = s.Name,
            };

        public CommandMetadataSurrogate ConvertToSurrogate(in CommandMetadata i) =>
            new()
            {
                Description = i.Description,
                Name = i.Name,
            };

        public BotAccountInfo ConvertFromSurrogate(in BotAccountInfoSurrogate s) =>
            new()
            {
                IsActive = s.IsActive,
                UserId = s.UserId,
                UserLogin = s.UserLogin,
            };

        public BotAccountInfoSurrogate ConvertToSurrogate(in BotAccountInfo i) =>
            new()
            {
                IsActive = i.IsActive,
                UserId = i.UserId,
                UserLogin = i.UserLogin,
            };

        public CustomCategoryDescription ConvertFromSurrogate(in CustomCategoryDescriptionSurrogate s) =>
            new()
            {
                TwitchCategoryId = s.TwitchCategoryId,
                Description = s.Description,
                Locale = s.Locale,
            };

        public CustomCategoryDescriptionSurrogate ConvertToSurrogate(in CustomCategoryDescription i) =>
            new()
            {
                TwitchCategoryId = i.TwitchCategoryId,
                Description = i.Description,
                Locale = i.Locale,
            };

        public UserRole ConvertFromSurrogate(in UserRoleSurrogate s) =>
            new()
            {
                ChannelId = s.ChannelId,
                ChannelName = s.ChannelName,
                Role = s.Role,
            };

        public UserRoleSurrogate ConvertToSurrogate(in UserRole i) =>
            new()
            {
                ChannelId = i.ChannelId,
                ChannelName = i.ChannelName,
                Role = i.Role,
            };
    }
}

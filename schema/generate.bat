:: Generates csharp classes to the parent dir
protoc.exe --csharp_out "../SCPDiscordBot/Interface" --csharp_out "../SCPDiscordPlugin/Interface" --proto_path "." "*.proto" "./BotToPlugin/*.proto" "./PluginToBot/*.proto"
timeout /t 5
pipeline {
  agent any
  stages {
    stage('Dependencies') {
      steps {
        sh 'nuget restore SCPDiscord.sln'
      }
    }
    stage('Build') {
      parallel {
        stage('Plugin') {
          steps {
            sh 'msbuild SCPDiscordPlugin/SCPDiscordPlugin.csproj -restore -p:PostBuildEvent='
          }
        }
        stage('Linux Bot') {
          steps {
            dir(path: 'SCPDiscordBot') {
              sh 'dotnet build --output bin/linux-x64 --configuration Release --runtime linux-x64'
            }
          }
        }
        stage('Windows Bot') {
          steps {
            dir(path: 'SCPDiscordBot') {
              sh 'dotnet build --output bin/win-x64 --configuration Release --runtime win-x64'
            }
          }
        }
      }
    }
    stage('Setup Output Dir') {
      steps {
        sh 'mkdir SCPDiscord'
        sh 'mkdir SCPDiscord/Plugin'
        sh 'mkdir SCPDiscord/Plugin/dependencies'
        sh 'mkdir SCPDiscord/Bot'
      }
    }
    stage('Package') {
      parallel {
        stage('Plugin') {
          steps {
            sh 'mv SCPDiscordPlugin/bin/SCPDiscord.dll SCPDiscord/Plugin/'
            sh 'mv SCPDiscordPlugin/bin/YamlDotNet.dll SCPDiscord/Plugin/dependencies'
            sh 'mv SCPDiscordPlugin/bin/Newtonsoft.Json.dll SCPDiscord/Plugin/dependencies'
          }
        }
        stage('Bot') {
          steps {
            dir(path: 'SCPDiscordBot') {
              sh 'warp-packer --arch linux-x64 --input_dir bin/linux-x64 --exec SCPDiscordBot --output ../SCPDiscord/Bot/SCPDiscordBot'
              sh 'warp-packer --arch windows-x64 --input_dir bin/win-x64 --exec SCPDiscordBot.exe --output ../SCPDiscord/Bot/SCPDiscordBot.exe'
              sh 'cp default_config.yml ../SCPDiscord/Bot/config.yml'
            }
          }
        }
      }
    }
    stage('Archive') {
      steps {
        sh 'zip -r SCPDiscord.zip SCPDiscord'
        archiveArtifacts(artifacts: 'SCPDiscord.zip', onlyIfSuccessful: true)
      }
    }
  }
}

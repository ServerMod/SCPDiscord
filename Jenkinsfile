pipeline {
  agent any
  stages {
    stage('Dependencies') {
      parallel {
        stage('Plugin') {
          steps {
            sh 'nuget restore SCPDiscord.sln'
          }
        }
        stage('Bot') {
          steps {
            dir(path: 'SCPDiscordBot') {
              sh 'npm install discord.js'
              sh 'npm install yaml'
            }

          }
        }
      }
    }
    stage('Build') {
      steps {
        sh 'msbuild SCPDiscordPlugin/SCPDiscordPlugin.csproj -restore -p:PostBuildEvent='
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
          }
        }
        stage('Bot') {
          steps {
            sh 'mv SCPDiscordBot/default_config.yml SCPDiscord/Bot/config.yml'
            sh 'mv SCPDiscordBot/node_modules SCPDiscord/Bot/node_modules'
            sh 'mv SCPDiscordBot/package.json SCPDiscord/Bot/'
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
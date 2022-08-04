[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)
[![Issues:Welcome](https://img.shields.io/badge/Issues-welcome-blue.svg)](https://github.com/DonnYep/QuarkAsset/wiki)

# QuarkAsset

QuarkAsset是一套轻量级的Unity资源加载方案。 内置AssetDatabae与AssetBundle加载模式。加载模式皆支持引用计数，可实时查看资源信息。快速开发阶段可采用AssetDatabase模式，无需进行ab构建。调试阶段可采用AssetBundle模式，轻松构建ab资源。在构建ab时，支持对资源的加密。在运行时加载资源时，可通过对应的密钥对资源进行解密。

* [QuarkAsset Wiki](https://github.com/DonnYep/QuarkAsset/wiki)<br/>

* UPM安装链接: https://github.com/DonnYep/QuarkAsset.git#upm
-----

<a name="标题导航"></a>

# 标题导航

- [QuarkEditor编辑器](#QuarkEditor编辑器)
  - [AssetDatabaseTab-BundleLabel](#QuarkEditor-AssetDatabaseTab-BundleLabel)
    - [设置AssetBundle](#设置AssetBundle)
  - [AssetDatabaseTab-ObjectLabel](#QuarkEditor-AssetDatabaseTab-ObjectLabel)
  - [AssetBundleTab](#QuarkEditor-AssetBundleTab)
  - [AssetDatasetTab](#QuarkEditor-AssetDatasetTab)
- [QuarkRuntime入口](#QuarkRuntime入口)
  - [未选择加载模式](#QuarkConfig未选择加载模式)
  - [AssetDatabase加载模式](#QuarkConfig-AssetDatabase加载模式)
  - [AssetBundle加载模式](#QuarkConfig-AssetBundle加载模式)
- [QuarkRuntime加载](#QuarkRuntime加载)
- [QuarkRuntime应用实例](#QuarkRuntime应用实例)
  - [自定义入口实例](#自定义入口实例)
- [QuarkResources加载](#QuarkResources加载)
  - [QuarkResources同步加载](#QuarkResources同步加载)
  - [QuarkResources异步加载](#QuarkResources异步加载)

<a name="QuarkEditor编辑器"></a>

## QuarkEditor编辑器

* Quark编辑器打开路径为`Window>QuarkAsset>QuarkAssetEdior`。

* 为实现团队协作，且灵活配置资源，Quark采用了`scriptableObject`作为资源寻址配置。

* 配置文件可在Quark的编辑器最上方，点击`CreateDataset`生成。

* 为方便阐述，下文统一采用`dataset`称呼Quark的资源寻址配置。

* Quark编辑器一共含三个选项卡，分别为:`AssetDatabaseTab`、`AssetBundleTab`与`AssetDatasetTab`。

* `AssetDatabaseTab`用于为`dataset`设置assetbundle与生成资源寻址信息.

* `AssetBundleTab`用于为`dataset`生成assetbundle资源与相关处理。

* `AssetDatasetTab`用于为`dataset`设置可识别的文件后缀名。

<a name="QuarkEditor-AssetDatabaseTab-BundleLabel"></a>

### AssetDatabaseTab-BundleLabel
![Quark_AssetDatabaseTab_BundleLabel](Docs/Images/Quark_AssetDatabaseTab_BundleLabel.png)

* 此页面显示dataset中包含的AssetBundle信息。


<a name="设置AssetBundle"></a>

#### 设置AssetBundle

* 将需要被打包为ab的文件夹或资源拖拽入bundle label窗口，被拖入的资源会自动生成ab名称。点击上方Build按钮，完成dataset资源识别。

* 每一条bundle信息都可通过点击右键生成的菜单进行操作。


<a name="QuarkEditor-AssetDatabaseTab-ObjectLabel"></a>

### AssetDatabaseTab-ObjectLabel
![Quark_AssetDatabaseTab_ObjectLabel](Docs/Images/Quark_AssetDatabaseTab_ObjectLabel.png)

* 此页面显示dataset中包含的AssetObject信息。

* 每一条object信息都可通过点击右键生成的菜单进行操作。


<a name="QuarkEditor-AssetBundleTab"></a>

### AssetBundleTab
![Quark_AssetBundleTab](Docs/Images/Quark_AssetBundleTab.png)

* 此Tab用于ab打包操作。

* 打包ab时需要选择ab所对应的平台。若需要拷贝到streamingAssets文件夹，则勾选CopyToStreamingAssets选项。

* 其余可选择使用默认预设。


<a name="QuarkEditor-AssetDatasetTab"></a>

### AssetDatasetTab
![Quark_AssetDatasetTab](Docs/Images/Quark_AssetDatasetTab.png)

* Dataset通过文件后缀名进行识别。 若需要自定义识别文件的后缀名，则可在此页面对文件后缀名列表进行增删操作。

-----

<a name="QuarkRuntime入口"></a>

## QuarkRuntime入口-QuarkConfig

* 选择合适初始化入口，挂载`QuarkConfig`脚本。

<a name="QuarkConfig未选择加载模式"></a>

### 未选择加载模式
![QuarkConfig_None](Docs/Images/QuarkConfig_None.png)


<a name="QuarkConfig-AssetDatabase加载模式"></a>

### AssetDatabase加载模式
![QuarkConfig_AssetDatabase](Docs/Images/QuarkConfig_AssetDatabase.png)

* 为`QuarkAssetDataset`赋予build好的dataset，即完成配置。

<a name="QuarkConfig-AssetBundle加载模式"></a>

### AssetBundle加载模式
![QuarkConfig_AssetBundle](Docs/Images/QuarkConfig_AssetBundle.png)

* AssetBundle模式下默认采用StreamingAssets路径加载。

* 若build后的ab文件处于StreamingAssets目录下的其他路径，勾选`EnableRelativeBuildPath`选项。勾选`EnableRelativeBuildPath`表示为采用`StreamingAssets`目录下的相对路径进行加载，需要在`RelativeBuildPath`中填入相对路径的地址。

* 若在资源构建阶段进行了加密，则在Encryption折叠选项下填入对应的密钥与数字，Quark会在runtime自动解密。

-----

<a name="QuarkRuntime加载"></a>

## QuarkRuntime加载

* Quark的加载类为`QuarkResources`，对应unity的Resources。

* 加载资源时输入的名称可采用以下三种范式：
    * 1、资源名。      
    * 2、资源名.后缀   
    * 3、资源路径      

* 加载时请注意以下内容：
    * 1、资源名大小写敏感的资源名 。示例：MyAudio
    * 2、后缀名大小写不敏感。示例：MyAudio.mp3或MyAudio.MP3
    * 3、资源路径大小写敏感，地址须以Assets/开头。示例：Assets/Audio/MyAudio.mp3。采用地址加载时，后缀名需要小写。
    
-----

<a name="QuarkRuntime应用实例"></a>

## QuarkRuntime应用实例

<a name="自定义入口实例"></a>

### 自定义入口实例

```csharp
using Quark;
using UnityEngine;
[DefaultExecutionOrder(-2000)]//建议延后入口执行优先级
public class GameLauncher : MonoBehaviour
{
    private void Awake()
    {
        QuarkResources.OnCompareManifestSuccess += OnCompareManifestSuccess;
        QuarkResources.OnCompareManifestFailure += OnCompareManifestFailure; 
    }
    private void Start()
    {
        if (QuarkResources.QuarkAssetLoadMode == QuarkLoadMode.AssetDatabase)
        {
            InitGame();
        }
    }
    private void OnCompareManifestSuccess(long size)
    {
        if (QuarkResources.QuarkAssetLoadMode == QuarkLoadMode.AssetBundle)
        {
            InitGame();
        }
    }
    private void OnCompareManifestFailure(string message)
    {
       //这里表示文件清单未被读取到，需要检查build后的文件是否存在以及路径是否正确
    }
    void InitGame()
    {
        //Quark资源初始化成功，可进行初始化！
    }
}
```
<a name="QuarkResources加载"></a>

### QuarkResources加载

<a name="QuarkResources同步加载"></a>

#### QuarkResources同步加载

```csharp
var myAudio = QuarkResources.LoadAsset<AudioClip>("MyAudio");// 资源名加载
var myText = QuarkResources.LoadAsset<TextAsset>("MyText.json");//资源名.后缀名加载
var myTexture = QuarkResources.LoadAsset<Texture>("Assets/Textures/MyTexture.png");//完整路径加载。注意路径需要采用 / ，\\不支持！
```

<a name="QuarkResources异步加载"></a>

#### QuarkResources异步加载

```csharp
QuarkResources.LoadAssetAsync<AudioClip>("MyAudio",res=> 
    //加载完成回调，获取资源, do sth
});
QuarkResources.LoadAssetAsync<TextAsset>("MyText.json",res=> 
{
    //加载完成回调，获取资源, do sth
});
QuarkResources.LoadAssetAsync<Texture>("Assets/Textures/MyTexture.png",res=> 
{
    //加载完成回调，获取资源, do sth
});
```
-----

**[回到最上层](#标题导航)**

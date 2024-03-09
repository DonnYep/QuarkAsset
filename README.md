[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)
[![Issues:Welcome](https://img.shields.io/badge/Issues-welcome-blue.svg)](https://github.com/DonnYep/QuarkAsset/wiki)

# QuarkAsset

QuarkAsset是一套轻量级的插件化Unity资源加载方案。 内置AssetDatabae与AssetBundle加载模式。加载模式皆支持引用计数，可实时查看资源信息。快速开发阶段可采用AssetDatabase模式，无需进行ab构建。调试阶段可采用AssetBundle模式，轻松构建ab资源。在构建ab时，支持对资源的加密。在Runtime加载资源时，可通过对应的密钥对资源进行解密。支持编辑内对包体分割，有效控制颗粒度。支持查看ab的依赖关系。内置BuildPipeline，可通过命令行实现自动化资源构建。支持构建预设，可版本控制构建profile预设。Jenkins自动化部署测试已通过。

-----

<a name="标题导航"></a>

# 标题导航
- [QuarkAsset](#quarkasset)
- [标题导航](#标题导航)
  - [UPM支持](#upm支持)
  - [QuarkEditor编辑器](#quarkeditor编辑器)
    - [AssetDatabaseTab](#assetdatabasetab)
      - [设置AssetBundle](#设置assetbundle)
      - [查看AssetBundle中的资源](#查看assetbundle中的资源)
      - [查看AssetBundle依赖](#查看assetbundle依赖)
      - [拆分AssetBundle](#拆分assetbundle)
        - [依据子文件夹拆分](#依据子文件夹拆分)
        - [依据每个资源个体拆分](#依据每个资源个体拆分)
    - [AssetBundleTab](#assetbundletab)
      - [使用本地设置](#使用本地设置)
      - [使用构建预设](#使用构建预设)
    - [AssetDatasetTab](#assetdatasettab)
  - [QuarkRuntime入口-QuarkConfig](#quarkruntime入口-quarkconfig)
    - [未选择加载模式](#未选择加载模式)
    - [AssetDatabase加载模式](#assetdatabase加载模式)
    - [AssetBundle加载模式](#assetbundle加载模式)
  - [QuarkRuntime应用实例](#quarkruntime应用实例)
    - [自定义入口实例](#自定义入口实例)
  - [QuarkRuntime](#quarkruntime)
    - [QuarkResources加载](#quarkresources加载)
      - [QuarkResources同步加载](#quarkresources同步加载)
      - [QuarkResources异步加载](#quarkresources异步加载)
    - [QuarkResources卸载](#quarkResources卸载)
      - [QuarkResources卸载单个资源](#quarkResources卸载单个资源)
      - [QuarkResources卸载assetbundle](#quarkResources卸载assetbundle)
  - [BuildPipeline](#buildpipeline)
    - [打包配置](#打包配置)
    - [命令行打包](#命令行打包)
  - [差量更新](#差量更新)
    - [文件清单合并](#文件清单合并)
  - [注意事项](#注意事项)

---

<a name="UPM支持"></a>

## UPM支持

* QuarkAsset是完全插件化的unity库，文件夹结构遵循unityPackage规范。

* UPM本地导入。选择Assets/QuarkAsset文件夹，拷贝到工程目录的Packages目录下，完成导入。

* UPM从git导入。url链接: https://github.com/DonnYep/QuarkAsset.git#upm 

---

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

<a name="QuarkEditor-AssetDatabaseTab"></a>

### AssetDatabaseTab

![Quark_AssetDatabaseTab](Docs/Images/Quark_AssetDatabaseTab.png)

* 此页面显示dataset中包含的AssetBundle信息。

* 每一条bundle信息都可通过点击右键生成的菜单进行操作。

* 每一条object信息都可通过点击右键生成的菜单进行操作。

<a name="设置AssetBundle"></a>

#### &nbsp; 设置AssetBundle

* 将需要被打包为ab的文件夹或资源拖拽入bundle label窗口，被拖入的资源会自动生成ab名称。点击上方Build按钮，完成dataset资源识别。
  
<a name="查看AssetBundle中的资源"></a>

#### &nbsp; 查看AssetBundle中的资源

* 选中需要查看的bundle，在右侧label中可缩放查看缩略图信息。
![Quark_AssetDatabaseTab_Preview](Docs/Images/Quark_AssetDatabaseTab_Preview.png)


<a name="查看AssetBundle依赖"></a>

#### &nbsp; 查看AssetBundle依赖

* 如图所示，选择显示区类别，可查看bundle的依赖关系以及subbundle信息。
![Quark_AssetDatabaseTab_Dependent](Docs/Images/Quark_AssetDatabaseTab_Dependent.png)


<a name="拆分AssetBundle"></a>

#### &nbsp; 拆分AssetBundle

<a name="依据子文件夹拆分"></a>

##### &nbsp; &nbsp; 依据子文件夹拆分

* Mark as splittable
![Quark_AssetDatabaseTab_SplitBundle](Docs/Images/Quark_AssetDatabaseTab_SplitBundle.png)

* 选择bundle，点击右键显示菜单，点击"Mark as splittable"选项，被选中的bundle会被标记为可分割的bundle，点击"Build"按钮，刷新信息。
  
* 分割bundle的逻辑为，若bundle存在子文件夹，则第一级的子文件夹被标记为新的bundle，在构建bundle时会生成新的bundle，同时父级bundle则不会再构建。

* `Splittable`与`Extract`是互斥的。即Splittable为true时，Extract无法为true，反之亦然。

<br>

<a name="依据每个资源个体拆分"></a>

##### &nbsp; &nbsp; 依据每个资源个体拆分

* Mark as extract
![Quark_AssetDatabaseTab_ExtractBundle](Docs/Images/Quark_AssetDatabaseTab_ExtractBundle.png)

* 选择bundle，点击右键显示菜单，点击"Mark as extract"选项，被选中的bundle会被标记为可分割的bundle，点击"Build"按钮，刷新信息。
  
* 当bundle被标记为Extract时，此bundle下的所有资源对象都将作为`独立的assetbundle`进行构建。此功能适用于变更频繁的资产。

* `Extract`与`Splittable`是互斥的。即Extract为true时，Splittable无法为true，反之亦然。

<div style="border-top: 1px solid black; margin-top: 20px; margin-bottom: 20px;"></div>

<a name="QuarkEditor-AssetBundleTab"></a>

### AssetBundleTab

* 此Tab用于ab打包操作。

* 打包ab时需要选择ab所对应的平台。若需要拷贝到streamingAssets文件夹，则勾选CopyToStreamingAssets选项。

* 打包ab配置有两种选择，第一种是`使用本地设置`，第二种是`使用构建预设`。
  * `本地设置`指打包ab的配置存储在本地设备。配置无法对其进行版本管理。
  * `构建预设`指生成一个预设文件，可以在不同机器上使用相同的配置进行ab打包，且可以被版本控制管理。

* 配置打包设置时，在更改需要变更的参数后，其余参数推荐使用默认值。

<a name="使用本地设置"></a>

#### &nbsp; 使用本地设置

* 如图，调整变更自己需要的参数。
![Quark_AssetBundleTab](Docs/Images/Quark_AssetBundleTab.png)

<a name="使用构建预设"></a>

#### &nbsp; 使用构建预设

* 使用构建预设，如图。勾选`Use build profile`选项，勾选后切换为预设构建。
![Quark_AssetBundleTab_UseBuildProfile](Docs/Images/Quark_AssetBundleTab_UseBuildProfile.png)

<br>

* 当构建预设不存在时，点击`+`按钮，生成一个构建预设。
![Quark_AssetBundleTab_CreateBuildProfile](Docs/Images/Quark_AssetBundleTab_CreateBuildProfile.png)

<br>

* 构建预设与本地配置大体相同。
![Quark_AssetBundleTab_BuildProfile](Docs/Images/Quark_AssetBundleTab_BuildProfile.png)


<div style="border-top: 1px solid black; margin-top: 20px; margin-bottom: 20px;"></div>

<a name="QuarkEditor-AssetDatasetTab"></a>

### AssetDatasetTab
![Quark_AssetDatasetTab](Docs/Images/Quark_AssetDatasetTab.png)

* Dataset通过文件后缀名进行识别。 此列表表示所有可以被quark识别的文件后缀名类型。若自定义的后缀名未被quark识别，则在此页面增加需要被识别的后缀名。

-----

<a name="QuarkRuntime入口"></a>

## QuarkRuntime入口-QuarkConfig

* 选择合适初始化入口，挂载`QuarkConfig`脚本。

<a name="QuarkConfig未选择加载模式"></a>

### 未选择加载模式
![QuarkConfig_None](Docs/Images/QuarkConfig_None.png)

<div style="border-top: 1px solid black; margin-top: 20px; margin-bottom: 20px;"></div>

<a name="QuarkConfig-AssetDatabase加载模式"></a>

### AssetDatabase加载模式
![QuarkConfig_AssetDatabase](Docs/Images/QuarkConfig_AssetDatabase.png)

* 为`QuarkAssetDataset`赋予build好的dataset，即完成配置。

<div style="border-top: 1px solid black; margin-top: 20px; margin-bottom: 20px;"></div>

<a name="QuarkConfig-AssetBundle加载模式"></a>

### AssetBundle加载模式
![QuarkConfig_AssetBundle](Docs/Images/QuarkConfig_AssetBundle.png)

* AssetBundle模式下默认采用StreamingAssets路径加载。

* 若build后的ab文件处于StreamingAssets目录下的其他路径，勾选`EnableRelativeBuildPath`选项。勾选`EnableRelativeBuildPath`表示为采用`StreamingAssets`目录下的相对路径进行加载，需要在`RelativeBuildPath`中填入相对路径的地址。

* 若在资源构建阶段进行了加密，则在Encryption折叠选项下填入对应的密钥与数字，Quark会在runtime自动解密。

* QuarkRuntime加载资源会自动计算引用计数，并根据引用计数加载或卸载assetbundle，无需手动管理ab资源。

-----
    

<a name="QuarkRuntime应用实例"></a>

## QuarkRuntime应用实例

<a name="自定义入口实例"></a>

### 自定义入口实例

```csharp
using Quark;
using Quark.Asset;
using System.IO;
using UnityEngine;
public class MyLauncher : MonoBehaviour
{
    /// <summary>
    /// QuarkAssetLoadMode下AssetDatabase模式所需的寻址数据。
    /// <see cref="Quark.QuarkLoadMode"/>
    /// </summary>
    [SerializeField] QuarkDataset quarkDataset;
    /// <summary>
    /// ab Build 的相对地址。
    /// </summary>
    [SerializeField] string streamingRelativeBuildPath;
    /// <summary>
    /// 对称加密密钥。
    /// </summary>
    [SerializeField] string manifestAesKey;
    /// <summary>
    /// 加密偏移量。
    /// </summary>
    [SerializeField] ulong encryptionOffset;
    private void Start()
    {
        //根据需要调用函数
    }
    void LanchAssetBundleMode()
    {
        //以AssetBundle模式启动
        var myPath = Path.Combine(Application.streamingAssetsPath, streamingRelativeBuildPath);
        QuarkResources.LaunchAssetBundleMode(myPath, () =>
        {
                //启动成功，进入游戏逻辑
            }, (errorMsg) =>
            {
                //启动失败，处理异常
            }, manifestAesKey, encryptionOffset);
    }
    void LanchAssetDatabaseMode()
    {
        //以AssetDatabase(unity editor 调试)模式启动。
        QuarkResources.LaunchAssetDatabaseMode(quarkDataset, () =>
        {
                //启动成功，进入游戏逻辑
            }, (errorMsg) =>
            {
                //启动失败，处理异常
            });
    }
}
```
-----

<a name="QuarkRuntime"></a>

## QuarkRuntime

* Quark的运行时类为为`QuarkResources`。

* 加载资源时输入的名称可采用以下三种范式：
    * 1、资源名。      
    * 2、资源名.后缀   
    * 3、资源路径      

* 加载&卸载时请注意以下内容：
    * 1、资源名大小写敏感的资源名 。示例：MyAudio
    * 2、后缀名大小写不敏感。示例：MyAudio.mp3或MyAudio.MP3
    * 3、资源路径大小写敏感，地址须以Assets/开头。示例：Assets/Audio/MyAudio.mp3。采用地址加载时，后缀名需要小写。

<a name="QuarkResources加载"></a>

### QuarkResources加载

<a name="QuarkResources同步加载"></a>

#### &nbsp; QuarkResources同步加载

```csharp
var myAudio = QuarkResources.LoadAsset<AudioClip>("MyAudio");// 资源名加载
var myText = QuarkResources.LoadAsset<TextAsset>("MyText.json");//资源名.后缀名加载
var myTexture = QuarkResources.LoadAsset<Texture>("Assets/Textures/MyTexture.png");//完整路径加载。注意路径需要采用 / ，\\不支持！
```

<a name="QuarkResources异步加载"></a>

#### &nbsp; QuarkResources异步加载

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
//异步加载场景
QuarkResources.LoadSceneAsync("MyScene",progress=>{Debug.Log(progress);},()=>{Debug.Log("Load Done");},false);
```



<a name="QuarkResources卸载"></a>

### QuarkResources卸载

<a name="QuarkResources资源单个卸载"></a>

#### &nbsp; QuarkResources卸载单个资源

* 卸载与加载相同，都支持三种三种范式：
    * 1、资源名。      
    * 2、资源名.后缀   
    * 3、资源路径      

```csharp
//资源名卸载单个资源
QuarkResources.UnloadAsset("MyAudio"); 
//资源名.后缀卸载单个资源
QuarkResources.UnloadAsset("MyText.json"); 
//资源路径卸载单个资源
QuarkResources.UnloadAsset("Assets/Textures/MyTexture.png"); 
//卸载场景
QuarkResources.UnloadSceneAsync("MyScene",progress=>{Debug.Log(progress);},()=>{Debug.Log("Unload Done")); 
```

<a name="QuarkResources卸载assetbundle"></a>

#### &nbsp; QuarkResources卸载assetbundle

```csharp
//卸载assetbundle
QuarkResources.UnloadAssetBundle("MyBundle",true); 
```
<div style="border-top: 1px solid black; margin-top: 20px; margin-bottom: 20px;"></div>

<a name="QuarkResources获取资源信息"></a>

### QuarkResources获取资源信息

```csharp
  //获取单个资源的状态信息
  QuarkResources.GetObjectInfo(myAssetName, out var info);
```
<a name="QuarkResources获取包体信息"></a>

### QuarkResources获取包体信息

```csharp
  //获取一个包体的状态信息
  QuarkResources.GetBundleInfo(myBundleName, out var info);
```

-----

<a name="BuildPipeline"></a>

## BuildPipeline

* Quark支持自动化流水线，提供构建静态函数，目前测试可使用Jenkins进行自动化部署。

<a name="打包配置"></a>

### 打包配置

* 选择一个项目中使用的QuarkAssetDataset类型文件，将文件名更改为QuarkAssetDataset后，放到Assets根目录下。放置完毕后路径应与如下相同。
```
Assets/QuarkAssetDataset.asset
```
<a name="命令行打包"></a>

### 命令行打包

* 命令行可调用的API如下：

```csharp
//打包指定平台的资源
QuarkBuildPipeline.BuildAssetBundle(BuildTarget buildTarget);
//通过预设进行构建
QuarkBuildPipeline.BuildAssetBundleByProfile(string datasetPath, string buildProfilePath);
```

* 详细的命令可查看`QuarkBuildPipeline.cs`类。

---

<a name="差量更新"></a>

## 差量更新

* Quark支持母包内嵌加热更下载。

* 目前支持StreamingAssets与persistentdatapath地址的同时加载。

<a name="文件清单合并"></a>

### 文件清单合并

* 使用如下API，对不同的文件清单进行合并，获得合并的文件清单后即可使用不同路径下的ab资源。
```csharp
QuarkUtility.Manifest.MergeManifest(srcManifest,diffManifest,out var mergedManifest);
```
---

<a name="注意事项"></a>

## 注意事项

* 自动化部署构建资源请根据打包策略选择合适的清理方式。建议采用所有资源重新构建的策略。

**[回到最上层](#标题导航)**

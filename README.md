

# **VRを用いた未知の食体験（視覚部分）**
本リポジトリは、VR技術を活用した「未知の食体験」に関する研究・開発のうち、**視覚部分の実装**を含むプロジェクトです。  
本体験の目的は、人類の普遍的な関心事である「食」に関する**新しい体験の可能性を探求すること**です。

## **🔍 プロジェクト概要**
VR技術の発展により、視覚・聴覚を活用した体験が広く普及していますが、**味覚・嗅覚・触覚** などの感覚提示技術はまだ十分に探求されていません。本プロジェクトでは、**「空気食体験」** という新しい食体験を実現し、VR技術の可能性を拡張することを目指しています。

本システムは以下の3つの要素で構成されています：
1. **視覚提示**（本リポジトリの実装部分）
2. **嗅覚提示**
3. **電気刺激による食感提示**

⚠ **本リポジトリでは、上記の「視覚提示」の部分のみを実装しており、嗅覚・電気刺激に関するコードは含まれていません。**

---

## **🎮 動作環境**
### **🔧 必要なハードウェア**
- **Meta Quest Pro**（Meta Quest 2 でも動作可能ですが、Quest Pro で最適化されています）
- **PC（Windows 10/11）**（PC VR モードでの開発・デバッグ推奨）
- **Oculus Link または Air Link**（PCVRモード使用時）

### **📦 必要なソフトウェア**
- **Unity 2021.3 LTS 以上**
- **Oculus Integration SDK**
- **OpenXR または Oculus XR Plugin**
- **Meta Quest Developer Hub（MQDH）**（Quest Pro でのビルド & デバッグに使用）

---

## **💻 インストール & セットアップ**
### **1️⃣ リポジトリをクローン**
```sh
git clone https://github.com/jasminelong/ppp.git
cd your-repo-name
```

### **2️⃣ Unity でプロジェクトを開く**
1. **Unity Hub** を開き、`ppp` を追加。
2. `Assets/Scenes/MainScene.unity` を開く。

### **3️⃣ Meta Quest Pro での動作確認**
#### **PC VR モード（Oculus Link / Air Link 使用）**
1. **Meta Quest Pro を PC に接続**（Oculus Link または Air Link）
2. **Unity でプレイモードを実行**
3. **VRヘッドセットで視覚提示の動作確認**

#### **Standalone Quest ビルド（Quest Pro 単体動作）**
1. **Oculus Integration SDK をインストール**
2. **「Android」プラットフォームに切り替え**
3. `Build & Run` で Quest Pro に直接ビルド



![微信图片_20250218154731](https://github.com/user-attachments/assets/13c7d6a9-e724-47e4-bd45-eee6288ee3a6)


https://ipsj.ixsq.nii.ac.jp/records/238797  
[2024年度メタバースコンペティション](https://vr.u-tokyo.ac.jp/2025/01/14/%e9%96%8b%e5%82%ac%e5%a0%b1%e5%91%8a-2024%e5%b9%b4%e5%ba%a6%e3%83%a1%e3%82%bf%e3%83%90%e3%83%bc%e3%82%b9%e3%82%b3%e3%83%b3%e3%83%9a%e3%83%86%e3%82%a3%e3%82%b7%e3%83%a7%e3%83%b3/)



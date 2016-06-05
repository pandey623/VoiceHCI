# VoiceHCI

### Human Computer Interaction (HCI) using voice for Unity.
音声を用いたヒューマンコンピュータインタラクション．

### Note;
This work was created for my experimental subject.

この作品は，大学の実験課題のために作成されたものです．

### How this work;
You can send an automatically analyzed emotional avatar that uses acoustic spectrum each other with socket communications.

ソケット通信を用いて，音声情報を解析してアバターを使って表情を送りあうことができます．

### How to use;
  At first, open "LoginScene.unity" then play. A server must set up the port number. Note that the NAT Portfowarding is not considered in this product. A client must IP Address (or you can input host name) and port number. After the server and client has connected each other, be quiet till 2 seconds left because the VoiceAnalyzer gets initial noises of microphone on that span. 

　LoginScene.unityを開き，実行します．サーバ側はポート番号を適切に設定して「Start Server」を押します．クライアント側は接続先IPとポート番号を指定して「Connect」を押します．WANを通す場合，ポート開放を適宜行ってください．
接続後，2秒程度の間，ノイズを拾う期間がありますので，この間はしゃべらないでください．

#### Reference:
hecomi, “Unity のオーディオの再生・エフェクト・解析周りについてまとめてみた,” 
[Online]. Available: http://tips.hecomi.com/entry/2014/11/11/021147. [Accessed: 5 6 2016].

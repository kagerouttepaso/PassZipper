# PassZipper 
[![Build status](https://ci.appveyor.com/api/projects/status/v8mcf37c0rr5xnq7?svg=true)](https://ci.appveyor.com/project/kagerouttepaso/passzipper)

パスワード付きジップファイルを作成するソフト

## 既知のバグ
アプリケーション本体のアイコンへD&Dしてファイル圧縮を行うとsetting.iniが圧縮しようとしたファイルに作成されてしまいます。

アプリケーションのショートカットを作成し、こちらにD&Dするようお願いします。


## インストール
こちらから最新版のReleaseのpackage.zipをダウンロードしてください。

[PassZipper Release](https://github.com/kagerouttepaso/PassZipper/releases "Releaseのページ")


## 使い方
アプリケーションに圧縮したいファイルやフォルダをまとめてD&Dして下さい。
20桁のパスワード付きのZIPファイルが出力されます。

ZIPファイルの出力先は後記の設定ファイルから変更できます。


## 設定
`./setting.ini` の一行目にファイルの出力先にしたいフォルダのフルパスを書いて下さい。




# その他
アプリケーションアイコンにはこちらを使用させてもらいました。

[monochromat.ico](http://spheresofa.net/blog/?p=524 "紹介ページ")

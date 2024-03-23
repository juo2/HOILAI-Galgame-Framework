mergeInto(LibraryManager.library,{   
    //Unity内自定义调用方法名 PostScore(string sceneName) 
    PostScore: function (sceneName) { 
     strs = Pointer_stringify(sceneName);   //字符串需用Pointer_stringify转换
     GetScore(strs);                        //前端自定义方法名GetScore(strs)
    },    
});
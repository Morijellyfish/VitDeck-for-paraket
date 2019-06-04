namespace VitDeck.Validator
{
    /// <summary>
    /// 検証ルールの基本となる抽象クラス
    /// </summary>
    public abstract class BaseRule : IRule
    {
        /// <summary>
        /// ルール名
        /// </summary>
        internal string name;
        /// <summary>
        /// 検証結果
        /// </summary>
        internal ValidationResult result;

        public BaseRule(string name)
        {
            this.name = name;
            result = new ValidationResult(name);
        }
        /// <summary>
        /// 検証結果を返す
        /// </summary>
        /// <returns>検証結果</returns>
        public ValidationResult GetResult()
        {
            return result;
        }

        internal void AddResultLog(string log)
        {
            result.AddResultLog(log);
        }

        internal void AddIssue(Issue issue)
        {
            result.AddIssue(issue);
        }
        /// <summary>
        /// 定められたルールに従って検証する。検証後にresultフィールドを結果として返す
        /// </summary>
        /// <param name="baseFolder">ベースフォルダの`Assets/`から始まる相対パス。</param>
        /// <returns>`result`に格納された検証結果</returns>
        public ValidationResult Validate(string baseFolder)
        {
            result = new ValidationResult(name);
            try
            {
                Logic(baseFolder);
            }
            catch (FatalValidationErrorException e)
            {
                result.AddIssue(new Issue(null, IssueLevel.Fatal, e.Message));
                throw e;
            }
            return result;
        }
        /// <summary>
        /// ルールの検証ロジック。
        /// </summary>
        /// <param name="baseFolder">ベースフォルダの`Assets/`から始まる相対パス。</param>
        internal abstract void Logic(string baseFolder);
    }
}
import os
import re
from datetime import datetime
import pytz
from github import Github

COMMIT_TYPES = {
    'feat': {'emoji': '✨', 'label': 'feature', 'description': 'New Feature'},
    'fix': {'emoji': '🐛', 'label': 'bug', 'description': 'Bug Fix'},
    'refactor': {'emoji': '♻️', 'label': 'refactor', 'description': 'Code Refactoring'},
    'docs': {'emoji': '📝', 'label': 'documentation', 'description': 'Documentation Update'},
    'test': {'emoji': '✅', 'label': 'test', 'description': 'Test Update'},
    'chore': {'emoji': '🔧', 'label': 'chore', 'description': 'Build/Config Update'},
    'style': {'emoji': '💄', 'label': 'style', 'description': 'Code Style Update'},
    'perf': {'emoji': '⚡️', 'label': 'performance', 'description': 'Performance Improvement'},
}

def parse_commit_message(message):
    """Parse commit message"""
    pattern = r'(?i)\[(.*?)\] (.*?)\n\n\[body\](.*?)(?:\n\n\[todo\](.*?))?(?:\n\n\[footer\](.*?))?$'
    match = re.search(pattern, message, re.DOTALL | re.IGNORECASE)
    if not match:
        return None
    
    commit_type = match.group(1).lower()
    type_info = COMMIT_TYPES.get(commit_type, {'emoji': '🔍', 'label': 'other', 'description': 'Other'})
    
    return {
        'type': commit_type,
        'type_info': type_info,
        'title': match.group(2),
        'body': match.group(3),
        'todo': match.group(4),
        'footer': match.group(5)
    }

def convert_to_checkbox_list(text):
    """Convert text to checkbox list"""
    if not text:
        return ''
    
    lines = []
    for line in text.strip().split('\n'):
        line = line.strip()
        if line.startswith(('-', '*')):
            line = f"- [ ] {line[1:].strip()}"
        lines.append(line)
    
    return '\n'.join(lines)

def create_commit_section(commit_data, branch, commit_sha, author, time_string):
    """Create commit section with details tag"""
    # Apply blockquote to each line of the body
    body_lines = [f"> {line}" for line in commit_data['body'].strip().split('\n')]
    quoted_body = '\n'.join(body_lines)
    
    # Apply blockquote to related issues if they exist
    related_issues = f"\n> **Related Issues:**\n> {commit_data['footer'].strip()}" if commit_data['footer'] else ''
    
    section = f'''> <details>
> <summary>💫 {time_string} - {commit_data['title'].strip()}</summary>
>
> Type: {commit_data['type']} ({commit_data['type_info']['description']})
> Commit: `{commit_sha}`
> Author: {author}
>
{quoted_body}{related_issues}
> </details>'''
    return section

def create_section(title, content):
    """Create collapsible section"""
    if not content:
        return ''
    
    return f'''<details>
<summary>{title}</summary>

{content}
</details>'''

def parse_existing_issue(body):
    """Parse existing issue body to extract branch commits and todos"""
    # Initialize result structure
    result = {
        'branches': {},
        'todos': []
    }
    
    # Parse branch section
    branch_pattern = r'<details>\s*<summary><h3 style="display: inline;">✨\s*(\w+)</h3></summary>(.*?)</details>'
    branch_blocks = re.finditer(branch_pattern, body, re.DOTALL)
    
    for block in branch_blocks:
        branch_name = block.group(1)
        branch_content = block.group(2).strip()
        result['branches'][branch_name] = branch_content
    
    # Parse Todo section
    todo_pattern = r'## 📝 Todo\s*\n\n(.*?)(?=\n\n<div align="center">|$)'
    todo_match = re.search(todo_pattern, body, re.DOTALL)
    if todo_match:
        todo_section = todo_match.group(1).strip()
        print(f"\n=== Current Issue's TODO List ===")
        if todo_section:
            todo_lines = [line.strip() for line in todo_section.split('\n') if line.strip()]
            for line in todo_lines:
                checkbox_match = re.match(r'- \[([ x])\] (.*)', line)
                if checkbox_match:
                    is_checked = checkbox_match.group(1) == 'x'
                    todo_text = checkbox_match.group(2)
                    result['todos'].append((is_checked, todo_text))
                    status = "✅ Completed" if is_checked else "⬜ Pending"
                    print(f"{status}: {todo_text}")
    
    return result

def merge_todos(existing_todos, new_todos):
    """Merge two lists of todos, avoiding duplicates and preserving order and state"""
    # Create a dictionary with todo text as key and (index, check state) as value
    todo_map = {}
    for idx, (checked, text) in enumerate(existing_todos):
        todo_map[text] = (idx, checked)
    
    # Initialize result list (with existing size)
    result = list(existing_todos)
    
    # Add new todos (check for duplicates)
    for checked, text in new_todos:
        if text in todo_map:
            # For existing todos, update check state only if newly checked
            idx, existing_checked = todo_map[text]
            if checked and not existing_checked:
                result[idx] = (True, text)
        else:
            # Add new todo
            result.append((checked, text))
            todo_map[text] = (len(result) - 1, checked)
    
    return result

def create_todo_section(todos):
    """Create todo section from list of (checked, text) tuples"""
    if not todos:
        return ''
    
    todo_lines = []
    for checked, text in todos:
        checkbox = '[x]' if checked else '[ ]'
        todo_lines.append(f'- {checkbox} {text}')
    
    return '\n'.join(todo_lines)

def get_previous_day_todos(repo, issue_label, current_date):
    """Get unchecked todos from the previous day's issue"""
    # Find previous day's issue
    previous_issues = repo.get_issues(state='open', labels=[issue_label])
    previous_todos = []
    previous_issue = None
    
    for issue in previous_issues:
        if issue.title.startswith('📅 Daily Development Log') and issue.title != f'📅 Daily Development Log ({current_date})':
            previous_issue = issue
            # Parse todos from previous issue
            existing_content = parse_existing_issue(issue.body)
            # Get only unchecked todos
            previous_todos = [(False, todo[1]) for todo in existing_content['todos'] if not todo[0]]
            # Close previous issue
            issue.edit(state='closed')
            break
    
    return previous_todos

def main():
    # Initialize GitHub token and environment variables
    github_token = os.environ['GITHUB_TOKEN']
    timezone = os.environ.get('TIMEZONE', 'Asia/Seoul')
    issue_prefix = os.environ.get('ISSUE_PREFIX', '📅')
    issue_label = os.environ.get('ISSUE_LABEL', 'daily-log')
    excluded_pattern = os.environ.get('EXCLUDED_COMMITS', '^(chore|docs|style):')

    # Initialize GitHub API client
    g = Github(github_token)
    
    # Get commit information from environment variables
    repository = os.environ['GITHUB_REPOSITORY']
    repo = g.get_repo(repository)
    commit_sha = os.environ['GITHUB_SHA']
    commit = repo.get_commit(commit_sha)
    branch = os.environ['GITHUB_REF'].replace('refs/heads/', '')

    # Get parent commits to find actual work commits
    if commit.commit.message.startswith('Merge'):
        # If it's a merge commit, get both parents
        parent_commits = commit.parents
        # Process the non-merge parent (usually the second parent is the feature branch)
        if len(parent_commits) > 1:
            commit = parent_commits[1]  # Use the second parent (feature branch commits)
    
    # Check for excluded commit types
    if re.match(excluded_pattern, commit.commit.message):
        print(f"Excluded commit type: {commit.commit.message}")
        return

    # Parse commit message
    commit_data = parse_commit_message(commit.commit.message)
    if not commit_data:
        print("Invalid commit message format")
        return

    # Get current time in specified timezone
    tz = pytz.timezone(timezone)
    now = datetime.now(tz)
    date_string = now.strftime('%Y-%m-%d')
    time_string = now.strftime('%H:%M:%S')

    # Get repository name from full path
    repo_name = repository.split('/')[-1]
    if repo_name.startswith('.'):
        repo_name = repo_name[1:]

    # Create issue title
    issue_title = f"{issue_prefix} Daily Development Log ({date_string}) - {repo_name}"

    # Search for existing issues
    issues = repo.get_issues(state='open', labels=[issue_label])
    today_issue = None
    previous_todos = []

    for issue in issues:
        if f"Daily Development Log ({date_string})" in issue.title:
            # Find today's issue
            today_issue = issue
        elif issue.title.startswith('📅 Daily Development Log'):
            # Get TODOs from previous day's issue
            existing_content = parse_existing_issue(issue.body)
            # Get only unchecked TODOs
            previous_todos.extend([(False, todo[1]) for todo in existing_content['todos'] if not todo[0]])
            # Close previous issue
            issue.edit(state='closed')
            print(f"Closed previous issue #{issue.number}")

    # Create commit section
    commit_details = create_commit_section(
        commit_data,
        branch,
        commit_sha,
        commit.commit.author.name,
        time_string
    )

    # Create todo section and merge with previous todos
    if today_issue:
        # Parse existing issue
        existing_content = parse_existing_issue(today_issue.body)
        print(f"\n=== TODO Statistics ===")
        print(f"Current TODOs in issue: {len(existing_content['todos'])} items")
        
        # Add new commit to branch section
        branch_title = branch.title()
        if branch_title in existing_content['branches']:
            existing_content['branches'][branch_title] = f"{existing_content['branches'][branch_title]}\n\n{commit_details}"
        else:
            existing_content['branches'][branch_title] = commit_details
        
        # Convert new todos from commit message
        new_todos = []
        if commit_data['todo']:
            todo_lines = convert_to_checkbox_list(commit_data['todo']).split('\n')
            new_todos = [(False, line[5:].strip()) for line in todo_lines if line.startswith('- [ ]')]
            print(f"New TODOs to be added: {len(new_todos)} items")
            print("\n=== New TODOs List ===")
            for _, todo_text in new_todos:
                print(f"⬜ {todo_text}")
        
        # Maintain existing todos while adding new ones
        all_todos = merge_todos(existing_content['todos'], new_todos)
        if previous_todos:
            print(f"\n=== TODOs Migrated from Previous Day ===")
            for _, todo_text in previous_todos:
                print(f"⬜ {todo_text}")
            all_todos = merge_todos(all_todos, previous_todos)
        
        print(f"\n=== Final Result ===")
        print(f"Total TODOs: {len(all_todos)} items")
        
        # Create updated body
        branch_sections = []
        for branch_name, branch_content in existing_content['branches'].items():
            branch_sections.append(f'''<details>
<summary><h3 style="display: inline;">✨ {branch_name}</h3></summary>

{branch_content}
</details>''')
        
        updated_body = f'''# {issue_title}

<div align="center">

## 📊 Branch Summary

</div>

{''.join(branch_sections)}

<div align="center">

## 📝 Todo

</div>

{create_todo_section(all_todos)}'''
        
        today_issue.edit(body=updated_body)
        print(f"Updated issue #{today_issue.number}")
    else:
        # For new issue, merge previous todos with new ones
        new_todos = []
        if commit_data['todo']:
            todo_lines = convert_to_checkbox_list(commit_data['todo']).split('\n')
            new_todos = [(False, line[5:].strip()) for line in todo_lines if line.startswith('- [ ]')]
        
        # Merge all todos: 새 todo + 이전 날짜 todo
        all_todos = merge_todos(new_todos, previous_todos)
        
        # Create initial body
        body = f'''# {issue_title}

<div align="center">

## 📊 Branch Summary

</div>

<details>
<summary><h3 style="display: inline;">✨ {branch.title()}</h3></summary>

{commit_details}
</details>

<div align="center">

## 📝 Todo

</div>

{create_todo_section(all_todos)}'''

        # Create new issue with initial content
        new_issue = repo.create_issue(
            title=issue_title,
            body=body,
            labels=[issue_label, f"branch:{branch}", f"type:{commit_data['type_info']['label']}"]
        )
        print(f"Created new issue #{new_issue.number}")

if __name__ == '__main__':
    main()
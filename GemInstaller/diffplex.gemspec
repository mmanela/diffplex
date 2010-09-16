version = File.read(File.expand_path("../VERSION",__FILE__)).strip

Gem::Specification.new do |spec|
	spec.platform	       = Gem::Platform::RUBY
	spec.name		       = 'diffplex'
	spec.version	       = version
	spec.files 		       = Dir['lib/**/*']
	
	spec.summary	       = 'DiffPlex is a .NET Diffing Library'
	spec.description     = <<-EOF
		DiffPlex is a diffing library that allows you to programatically create
		text diffs. DiffPlex is a fast and tested library which is used by CodePlex
		to provide its source code diffing functionality.
	EOF
	
	spec.authors		   = 'Matthew Manela'
	spec.email		       = 'N/A'
	spec.homepage	 	   = 'http://diffplex.codeplex.com'
	spec.rubyforge_project = 'diffplex'
end